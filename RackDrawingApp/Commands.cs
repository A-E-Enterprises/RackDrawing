using AppColorTheme;
using DrawingControl;
using MaterialDesignThemes.Wpf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using RackDrawingApp.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RackDrawingApp
{
	/// <summary>
	/// Base class for WPF-commands
	/// </summary>
	public class Command : ICommand
	{
		public Command(PackIconKind iconKind, string strDescription)
		{
			IconKind = iconKind;
			Description = strDescription;
		}

		//=============================================================================
		public PackIconKind IconKind { get; set; }

		//=============================================================================
		public string Description { get; set; }

		//=============================================================================
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		//=============================================================================
		public bool CanExecute(object parameter)
		{
			return _CanExecute(parameter);
		}
		protected virtual bool _CanExecute(object parameter)
		{
			return true;
		}

		//=============================================================================
		public void Execute(object parameter)
		{
			_Execute(parameter);
		}
		protected virtual void _Execute(object parameter) { }
	}

	public class Command_NewDocument : Command
	{
		public Command_NewDocument()
			: base(PackIconKind.CropPortrait, "New document") { }

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			if (vm.CurrentDocument == null)
				return;

			// Double call DialogHost.Show() throw exceptions
			// For example - rename sheet and close application.
			if (vm.DlgHost != null && vm.DlgHost.IsOpen)
				return;

			// is here unsaved documents?
			bool bUnsaved = vm.CurrentDocument.HasChanges;
			if (bUnsaved)
			{
				SaveChangesDialog_ViewModel saveChangesDialogVM = new SaveChangesDialog_ViewModel();
				saveChangesDialogVM.Text = "This document is unsaved and contains some changes. Some data will be lost possibly.";
				saveChangesDialogVM.IsSaveButtonVisible = false;

				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(saveChangesDialogVM);

				//show the dialog
				// true - save
				// false - cancel
				// null - continue
				var result = await DialogHost.Show(saveChangesDialog);

				if (result is bool && !(bool)result)
					return;
			}

			vm.CurrentDocument = vm.CreateNewDocument();
		}
	}

	public class Command_NewSheet : Command
	{
		public Command_NewSheet()
			: base(PackIconKind.NotePlus, "NewSheet") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument currDoc = vm.CurrentDocument;
			if (currDoc == null)
				return;

			currDoc.Set_ShowAdvancedProperties(false, false);
			currDoc.AddSheet(new DrawingSheet(vm.CurrentDocument), true);
			currDoc.MarkStateChanged();
		}
	}

	public class Command_Open : Command
	{
		public Command_Open()
			: base(PackIconKind.Folder, "Open") { }

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			string strFileName = FileUtils._OpenFileDialog(DrawingDocument.FILE_FILTER, DrawingDocument.FILE_EXTENSION, null);
			if (string.IsNullOrEmpty(strFileName))
				return;

			bool result = await vm.OpenDrawing(strFileName);
		}
	}

	public class Command_Save : Command
	{
		public Command_Save()
			: base(PackIconKind.ContentSave, "Save") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

#if (!DEBUG)
			// Dont allow user to save document with empty or not correct ENQ number.
			// But it is available under DEBUG, because need to create document with empty ENQ number which will be used as a default template - "DocumentTemplate.rda".

			// ENQ number is neccessaru for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}
			// Check that ENQ number can be used as a filename for the text file.
			if (curDoc.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				vm.DisplayMessageDialog("ENQ number will be used as a filename, but it has incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
				return;
			}
#endif

			string strFilePath = string.Empty;
			if (curDoc.IsItNewDocument || curDoc.NameWithoutExtension != curDoc.CustomerENQ)
			{
				string strOldPath = null;
				if (!curDoc.IsItNewDocument && !string.IsNullOrEmpty(curDoc.Path))
					strOldPath = curDoc.Path;

				string strDefaultFileName = FileUtils.BuildDefaultFileName(curDoc.CustomerENQ, curDoc.DocumentRevision);
				strFilePath = FileUtils._SaveFileDialog(DrawingDocument.FILE_FILTER, DrawingDocument.FILE_EXTENSION, curDoc.CustomerENQ, strDefaultFileName, strOldPath);
				if (string.IsNullOrEmpty(strFilePath))
					return;
			}

			curDoc.Save(strFilePath);
			vm.OnDocumentSaved();
		}
	}

	public class Command_SaveAs : Command
	{
		public Command_SaveAs()
			: base(PackIconKind.ContentSaveAll, "Save As") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

#if (!DEBUG)
			// Dont allow user to save document with empty or not correct ENQ number.
			// But it is available under DEBUG, because need to create document with empty ENQ number which will be used as a default template - "DocumentTemplate.rda".

			// ENQ number is neccessaru for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}
			// Check that ENQ number can be used as a filename for the text file.
			if (curDoc.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				vm.DisplayMessageDialog("ENQ number will be used as a filename, but it has incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
				return;
			}
#endif

			string strOldPath = null;
			if (!curDoc.IsItNewDocument && !string.IsNullOrEmpty(curDoc.Path))
				strOldPath = curDoc.Path;

			string strDefaultFileName = FileUtils.BuildDefaultFileName(curDoc.CustomerENQ, curDoc.DocumentRevision);
			string strFilePath = FileUtils._SaveFileDialog(DrawingDocument.FILE_FILTER, DrawingDocument.FILE_EXTENSION, curDoc.CustomerENQ, strDefaultFileName, strOldPath);
			if (string.IsNullOrEmpty(strFilePath))
				return;

			curDoc.Save(strFilePath);
			vm.OnDocumentSaved();
		}
	}

	/// <summary>
	/// "Save as" with increase document revision
	/// </summary>
	public class Command_IncreaseRevisionSave : Command
	{
		public Command_IncreaseRevisionSave()
			: base(PackIconKind.ContentSaveAll, "Increase revision") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

#if (!DEBUG)
			// Dont allow user to save document with empty or not correct ENQ number.
			// But it is available under DEBUG, because need to create document with empty ENQ number which will be used as a default template - "DocumentTemplate.rda".

			// ENQ number is neccessaru for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}
			// Check that ENQ number can be used as a filename for the text file.
			if (curDoc.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				vm.DisplayMessageDialog("ENQ number will be used as a filename, but it has incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
				return;
			}
#endif

			string strOldPath = null;
			if (!curDoc.IsItNewDocument && !string.IsNullOrEmpty(curDoc.Path))
				strOldPath = curDoc.Path;

			string strDefaultFileName = FileUtils.BuildDefaultFileName(curDoc.CustomerENQ, curDoc.DocumentRevision + 1);
			string strFilePath = FileUtils._SaveFileDialog(DrawingDocument.FILE_FILTER, DrawingDocument.FILE_EXTENSION, curDoc.CustomerENQ, strDefaultFileName, strOldPath);
			if (string.IsNullOrEmpty(strFilePath))
				return;

			curDoc.Save(strFilePath, true);
			vm.OnDocumentSaved();
		}
	}

	public class Command_Undo : Command
	{
		public Command_Undo()
			: base(PackIconKind.UndoVariant, "Undo") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			return curDoc.CanUndo;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			curDoc.Undo();
		}
	}

	public class Command_Redo : Command
	{
		public Command_Redo()
			: base(PackIconKind.RedoVariant, "Redo") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			return curDoc.CanRedo;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			curDoc.Redo();
		}
	}

	public class Command_ExportToTXT : Command
	{
		public Command_ExportToTXT()
			: base(PackIconKind.FileExport, "Export to text file") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			if (curDoc.ContainsTieBeamsErrors)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			if (curDoc.ContainsTieBeamsErrors)
				return;

			// Dont allow user to export document with empty or not correct ENQ number.
			// ENQ number is neccessaru for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}
			// Check that ENQ number can be used as a filename for the text file.
			if (curDoc.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				vm.DisplayMessageDialog("ENQ number will be used as a filename, but it has incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
				return;
			}

			string strFilePath = FileUtils._SaveFileDialog("Text file |*.txt", ".txt", curDoc.CustomerENQ, curDoc.CustomerENQ);
			if (string.IsNullOrEmpty(strFilePath))
				return;

			curDoc.ExportToTxt(strFilePath);
		}
	}

	/// <summary>
	/// Display dialog with customer info data
	/// </summary>
	public class Command_CustomerInfo : Command
	{
		public Command_CustomerInfo()
			: base(PackIconKind.Account, "Customer info") { }

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			// Double call DialogHost.Show() throw exceptions
			// For example - rename sheet and close application.
			if (vm.DlgHost != null && vm.DlgHost.IsOpen)
				return;

			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);

			CustomerInfoDialog customerInfoDialog = new CustomerInfoDialog(curDoc);

			//show the dialog
			// true - save
			// false - cancel
			// null - continue
			var result = await DialogHost.Show(customerInfoDialog);

			// Remove all incorrect characters from the ENQ number.
			if(!string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				try
				{
					string strInvalidCharacters = new string(Path.GetInvalidFileNameChars());
					Regex r = new Regex(string.Format("[{0}]", Regex.Escape(strInvalidCharacters)));
					curDoc.CustomerENQ = r.Replace(curDoc.CustomerENQ, "");
				}
				catch { }
			}

			// Mark state changed
			curDoc.MarkStateChanged();
		}
	}

	/// <summary>
	/// Export all sheets layout and each Rack index configuration.
	/// </summary>
	public class Command_ExportImages : Command
	{
		public Command_ExportImages()
			: base(PackIconKind.Image, "Export images") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			if (vm.DrawingControl == null)
				return false;

			if (Utils.FLE(vm.DrawingControl.ActualWidth, 0.0) || Utils.FLE(vm.DrawingControl.ActualHeight, 0.0))
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			if (curDoc.ContainsTieBeamsErrors)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			if (vm.DrawingControl == null)
				return;

			if (Utils.FLE(vm.DrawingControl.ActualWidth, 0.0) || Utils.FLE(vm.DrawingControl.ActualHeight, 0.0))
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			if (curDoc.ContainsTieBeamsErrors)
				return;

			// ENQ number is neccessary for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}

			// remove extension(.rda) and "*" symbol for new document
			string strDocNameWithoutExtensions = curDoc.DisplayName.Replace(".rda", string.Empty).Replace("*", string.Empty);

			// select folder
			string strFolder = string.Empty;
			using (FolderBrowserDialog fbd = new FolderBrowserDialog())
			{
				string strDefDir = FileUtils.BuildDefaultDirectory(curDoc.CustomerENQ);
				fbd.SelectedPath = strDefDir;

				DialogResult result = fbd.ShowDialog();

				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
					strFolder = fbd.SelectedPath;
			}

			if (string.IsNullOrEmpty(strFolder))
				return;

			// export all sheets
			if (curDoc.Sheets == null)
				return;

			// Application theme color should not be applied to PDF export picture.
			// So, save the current theme, set default light theme, export and set current theme back.
			ColorTheme oldTheme = new DefaultLightTheme();
			if (CurrentTheme.CurrentColorTheme != null)
				oldTheme = CurrentTheme.CurrentColorTheme.Clone() as ColorTheme;
			CurrentTheme.CurrentColorTheme = new DefaultLightTheme();
			// Aisle space should be very light for PDF and pictures export
			CurrentTheme.CurrentColorTheme.GeometryColorsTheme.SetGeometryColor(eColorType.eFill_AisleSpace, Colors.Ivory);

			// For PDF and picture export watermark should be lighter
			double oldWatermarkOpacity = WatermarkInfo.Opacity;
			WatermarkInfo.Opacity = 0.07;

			try
			{
				// list of rack with unique index
				// need for export
				List<Rack> racksList = new List<Rack>();

				// export sheets layout
				foreach (DrawingSheet sheet in curDoc.Sheets)
				{
					if (sheet == null)
						continue;

					if (sheet.Rectangles == null)
						continue;

					if (sheet.Rectangles.Count == 0)
						continue;

					// calculate the size of picture.
					// it should have the same length\height ratio as a sheet.
					// but sheet can has 20000 as a length value, so need to limit max image length\height with 2000 pixels.
					int imageLength = 0;
					int imageHeight = 0;
					if (Utils.FLT(sheet.Length, maxImageSize) && Utils.FLT(sheet.Width, maxImageSize))
					{
						imageLength = (int)Math.Ceiling((decimal)sheet.Length);
						imageHeight = (int)Math.Ceiling((decimal)sheet.Width);
					}
					else
					{
						double sizeScale = 1.0;
						if (Utils.FGT(sheet.Length, sheet.Width))
							sizeScale = (double)maxImageSize / sheet.Length;
						else
							sizeScale = (double)maxImageSize / sheet.Width;
						//
						imageLength = (int)Math.Ceiling(sizeScale * sheet.Length);
						imageHeight = (int)Math.Ceiling(sizeScale * sheet.Width);
					}

					// export layout picture
					if (Utils.FLE(sheet.Length, curDoc.PrintingSheetElevationMaxLength) && Utils.FLE(sheet.Width, curDoc.PrintingSheetElevationMaxHeight))
					{
						DrawingVisual sheetVisual = CreateSheetLayoutVisual(sheet, vm.DrawingControl.WatermarkImage, imageLength, imageHeight, false, true);
						if (sheetVisual != null)
						{
							string strImagePath = Path.Combine(strFolder, strDocNameWithoutExtensions + "_" + sheet.Name + ".jpeg");
							_ExportDrawingVisual(sheetVisual, strImagePath, imageLength, imageHeight);
						}
					}
					else
					{
						double minUnitsPerPixel = 0.0;

						int totalImageLength = imageLength - 2 * Command_ExportImages.m_ExtraSpaceForDimension;
						int totalImageHeight = imageHeight - 2 * Command_ExportImages.m_ExtraSpaceForDimension;

						double unitPerPixel_SheetLayout = ImageCoordinateSystem.GetMaxUnitsPerPixel(totalImageLength, totalImageHeight, sheet.Length, sheet.Width);

						double unitPerPixel_HorizSheetElevation = 0.0;
						double unitPerPixel_VertSheetElevation = 0.0;
						//
						List<Rack> horizontalSheetElevationRacksList = null;
						List<Rack> verticalSheetElevationRacksList = null;
						double horizontalSheetElevation_BiggestRackHeight = 0.0;
						double verticalSheetElevation_BiggestRackHeight = 0.0;
						// Sheet layout and sheet elevations pictures should have the same scale.
						if (sheet.GetSheetElevations(out horizontalSheetElevationRacksList, out verticalSheetElevationRacksList, out horizontalSheetElevation_BiggestRackHeight, out verticalSheetElevation_BiggestRackHeight))
						{
							if (horizontalSheetElevationRacksList != null && horizontalSheetElevationRacksList.Count > 0 && Utils.FGT(horizontalSheetElevation_BiggestRackHeight, 0.0))
								unitPerPixel_HorizSheetElevation = ImageCoordinateSystem.GetMaxUnitsPerPixel(totalImageLength, totalImageHeight, sheet.Length, horizontalSheetElevation_BiggestRackHeight);

							if (verticalSheetElevationRacksList != null && verticalSheetElevationRacksList.Count > 0 && Utils.FGT(verticalSheetElevation_BiggestRackHeight, 0.0))
								unitPerPixel_VertSheetElevation = ImageCoordinateSystem.GetMaxUnitsPerPixel(totalImageLength, totalImageHeight, verticalSheetElevation_BiggestRackHeight, sheet.Width);
						}

						minUnitsPerPixel = Math.Max(unitPerPixel_SheetLayout, Math.Max(unitPerPixel_HorizSheetElevation, unitPerPixel_VertSheetElevation));

						DrawingVisual sheetVisual = Command_ExportImages.CreateSheetLayoutVisual(sheet, vm.DrawingControl.WatermarkImage, imageLength, imageHeight, false, false, minUnitsPerPixel);
						if (sheetVisual != null)
						{
							string strImagePath = Path.Combine(strFolder, strDocNameWithoutExtensions + "_" + sheet.Name + ".jpeg");
							_ExportDrawingVisual(sheetVisual, strImagePath, imageLength, imageHeight);
						}

						if (Utils.FGT(unitPerPixel_HorizSheetElevation, 0.0))
						{
							DrawingVisual horizSheetElevVisual = Command_ExportImages.CreateSheetElevationVisual(sheet, vm.DrawingControl.WatermarkImage, imageLength, imageHeight, true, horizontalSheetElevationRacksList, horizontalSheetElevation_BiggestRackHeight, minUnitsPerPixel);
							{
								string strImagePath = Path.Combine(strFolder, strDocNameWithoutExtensions + "_" + sheet.Name + "_HorizontalSheetElevation.jpeg");
								_ExportDrawingVisual(horizSheetElevVisual, strImagePath, imageLength, imageHeight);
							}
						}
						if (Utils.FGT(unitPerPixel_VertSheetElevation, 0.0))
						{
							DrawingVisual vertSheetElevVisual = Command_ExportImages.CreateSheetElevationVisual(sheet, vm.DrawingControl.WatermarkImage, imageLength, imageHeight, false, verticalSheetElevationRacksList, verticalSheetElevation_BiggestRackHeight, minUnitsPerPixel);
							if (vertSheetElevVisual != null)
							{
								string strImagePath = Path.Combine(strFolder, strDocNameWithoutExtensions + "_" + sheet.Name + "_VerticalSheetElevation.jpeg");
								_ExportDrawingVisual(vertSheetElevVisual, strImagePath, imageLength, imageHeight);
							}
						}
					}

					// export layout with dimensions picture
					DrawingVisual sheetVisualWithSizes = CreateSheetLayoutVisual(sheet, vm.DrawingControl.WatermarkImage, imageLength, imageHeight, true, false);
					if (sheetVisualWithSizes != null)
					{
						string strImagePath = Path.Combine(strFolder, strDocNameWithoutExtensions + "_" + sheet.Name + "_WithDimensions.jpeg");
						_ExportDrawingVisual(sheetVisualWithSizes, strImagePath, imageLength, imageHeight);
					}

					// check racks for export
					foreach (BaseRectangleGeometry geom in sheet.Rectangles)
					{
						if (geom == null)
							continue;

						Rack rackGeom = geom as Rack;
						if (rackGeom == null)
							continue;

						Rack foundRack = racksList.Find(r => r.SizeIndex == rackGeom.SizeIndex && r.IsFirstInRowColumn == rackGeom.IsFirstInRowColumn);
						if (foundRack == null)
							racksList.Add(rackGeom);
					}
				}

				// sort racks list
				RackComparer rc = new RackComparer();
				racksList.Sort(rc);
				// export rack advanced properties
				foreach (Rack rack in racksList)
				{
					if (rack == null)
						continue;

					// create image
					DrawingVisual rackVisual = CreateRackAdvancedPropsVisual(rack, vm.DrawingControl.WatermarkImage, maxImageSize, maxImageSize);
					if (rackVisual == null)
						continue;
					// rack.Text contains text that displayed over rack in the sheet layout
					string strImagePath = Path.Combine(strFolder, strDocNameWithoutExtensions + "_" + rack.Text + ".jpeg");
					_ExportDrawingVisual(rackVisual, strImagePath, maxImageSize, maxImageSize);
				}
			}
			catch { }
			finally
			{
				if (oldTheme != null)
					CurrentTheme.CurrentColorTheme = oldTheme;

				// restore old watermark opacity value
				WatermarkInfo.Opacity = oldWatermarkOpacity;
			}
		}

		//=============================================================================
		/// <summary>
		/// Returns bitmap image source for passed visual.
		/// </summary>
		public static RenderTargetBitmap _GetBmpFromVisual(Visual visual, int imageLength, int imageHeight)
		{
			if (visual == null)
				return null;

			if (imageLength == 0 || imageHeight == 0)
				return null;

			// Make it before call bitmap.Render(), because otherwise bitmap.Render() can throw OutOfMemory exception.
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// dont pass sheet.Length and sheet.Width as image size, because they can has 20000 units value
			RenderTargetBitmap bitmap = new RenderTargetBitmap(imageLength, imageHeight, 96, 96, PixelFormats.Default);
			bitmap.Render(visual);

			return bitmap;
		}

		//=============================================================================
		/// <summary>
		/// Export DrawingVisual in the separate image file.
		/// </summary>
		private static void _ExportDrawingVisual(Visual visualForExport, string strFilePath, int imageLength, int imageHeight)
		{
			if (visualForExport == null)
				return;

			if (imageLength == 0 || imageHeight == 0)
				return;

			// dont pass sheet.Length and sheet.Width as image size, because they can has 20000 units value
			var bitmap = new RenderTargetBitmap(imageLength, imageHeight, 96, 96, PixelFormats.Default);
			bitmap.Render(visualForExport);
			//
			var encoder = new JpegBitmapEncoder();//new PngBitmapEncoder();
												  // flip vertical, because WPF has (0, 0)-point in the top left corner, but its need (0, 0)-point in the left bot corner
			encoder.Frames.Add(BitmapFrame.Create(bitmap));
			//
			using (var file = File.OpenWrite(strFilePath))
			{
				encoder.Save(file);
			}
		}

		//=============================================================================
		/// <summary>
		/// Pixels reserved for sheet length and height dimensions.
		/// </summary>
		public static int m_ExtraSpaceForDimension = 150;
		private static int m_DimensionOffsetInPixels = 20;
		/// <summary>
		/// Sheet elevation picture offset(in pixels) from sheet sheet layout picture.
		/// </summary>
		private static int m_SheetElevationsOffsetInPixels = 150;
		// max side of result images, take the max side for A3 sheet.
		public static int maxImageSize = 4200;

		/// <summary>
		/// Contains exporting font size ratio to drawing image size
		/// </summary>
		private class ExportFontSizeDependencies
        {
			private class FontSizeDependency
            {
                public int ImageWidth { get; private set; }
                public int ImageHeight { get; private set; }
                public double FontSize { get; private set; }

				public FontSizeDependency(int imageWidth, int imageHeight, double fontSize)
                {
					ImageWidth = imageWidth;
					ImageHeight = imageHeight;
					FontSize = fontSize;

				}
            }

			public static double _defaultExportFontSize = 10;

			private static List<FontSizeDependency> _widthToSizeDependencies = new List<FontSizeDependency> {
				new FontSizeDependency(30000, 20000, 35),
				new FontSizeDependency(70000, 55000, 15),
				new FontSizeDependency(200000, 100000, 5)
			};

			/// <summary>
			/// Get exporting font size based on drawing image size
			/// </summary>
			/// <param name="imageWidth">Drawing image width (X-axis)</param>
			/// <param name="imageHeight">Drawing image height (Y-axis)</param>
			public static double GetFontSize(int imageWidth, int imageHeight)
            {
				double lastFitWidth = _defaultExportFontSize;

				foreach (FontSizeDependency dependency in _widthToSizeDependencies)
                {
					if (imageWidth < dependency.ImageWidth || imageHeight < dependency.ImageHeight)
						break;

					lastFitWidth = dependency.FontSize;
				}

				return lastFitWidth;
            }
        }

		/// <summary>
		/// Create sheet visual, which contains all sheet geometry.
		/// </summary>
		/// <param name="bExportGeometryDimensions">
		/// If true then all geometry is partial transparent and geometry dimensions(length and depth) are displayed over geometry.
		/// Also sheet length and depth dimensions are displayed.
		/// </param>
		/// <returns></returns>
		public static DrawingVisual CreateSheetLayoutVisual(
			DrawingSheet sheet,
			ImageSource watermarkImage,
			int imageLength,
			int imageHeight,
			bool bExportGeometryDimensions,
			bool bIncludeSheetElevationsPictures,
			double minUnitsPerPixel = 0.0)
		{
			double exportFontSize = ExportFontSizeDependencies.GetFontSize(imageLength, imageHeight);

			if (sheet == null)
				return null;

			if (sheet.Document == null)
				return null;

			if (sheet.Length == 0 || sheet.Width == 0)
				return null;

			if (sheet.Rectangles == null)
				return null;

			//
			GC.Collect();
			GC.WaitForPendingFinalizers();

			IGeomDisplaySettings geomDisplaySettings = DefaultGeomDisplaySettings.GetInstance();
			geomDisplaySettings.TextFontSize = exportFontSize;

			//=============================================================== TODO: collect dimensions ===============================================================
			List<DimensionData> dimensions = new List<DimensionData>();
			List<Rect> textColisionlist = new List<Rect>();

			//
			int totalImageLength = imageLength;
			int totalImageHeight = imageHeight;
			// left and right extra space
			totalImageLength -= 2 * m_ExtraSpaceForDimension;
			// top and bot extra space
			totalImageHeight -= 2 * m_ExtraSpaceForDimension;
			Vector offsetInPixels = new Vector(m_ExtraSpaceForDimension, m_ExtraSpaceForDimension);

			// include sheet elevations pictures
			// additional length and height in global sheet units(not pixels) reserved for horizontal and vertical sheet elevations pictures
			double additionalLength_SheetElevation = 0.0;
			double additionalHeight_SheetElevation = 0.0;
			//
			List<Rack> horizontalSheetElevationRacksList = null;
			List<Rack> verticalSheetElevationRacksList = null;
			double horizontalSheetElevation_BiggestRackHeight = 0.0;
			double verticalSheetElevation_BiggestRackHeight = 0.0;
			if (bIncludeSheetElevationsPictures)
			{
				if (sheet.GetSheetElevations(out horizontalSheetElevationRacksList, out verticalSheetElevationRacksList, out horizontalSheetElevation_BiggestRackHeight, out verticalSheetElevation_BiggestRackHeight) && horizontalSheetElevationRacksList != null && verticalSheetElevationRacksList != null)
				{
					if (Utils.FGT(horizontalSheetElevation_BiggestRackHeight, 0.0))
					{
						additionalHeight_SheetElevation = horizontalSheetElevation_BiggestRackHeight;

						// offset vetween sheet elevation picture and sheet layot picture
						totalImageHeight -= m_SheetElevationsOffsetInPixels;
						// space for sheet elevation dimsension
						totalImageHeight -= m_ExtraSpaceForDimension;
					}
					if (Utils.FGT(verticalSheetElevation_BiggestRackHeight, 0.0))
					{
						additionalLength_SheetElevation = verticalSheetElevation_BiggestRackHeight;

						// offset vetween sheet elevation picture and sheet layot picture
						totalImageLength -= m_SheetElevationsOffsetInPixels;
						// space for sheet elevation dimsension
						totalImageLength -= m_ExtraSpaceForDimension;
					}
				}
			}

			ImageCoordinateSystem ics = new ImageCoordinateSystem(totalImageLength, totalImageHeight, offsetInPixels, new System.Windows.Size(sheet.Length + additionalLength_SheetElevation, sheet.Width + additionalHeight_SheetElevation), minUnitsPerPixel);

			//
			DrawingVisual visual = new DrawingVisual();
			using (DrawingContext dc = visual.RenderOpen())
			{
				// draw background
				dc.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(new System.Windows.Point(0.0, 0.0), new System.Windows.Point(imageLength, imageHeight)));
				// draw sheet border
				System.Windows.Media.Pen sheetBorder = null;
				//if (exportDimensions)
				sheetBorder = new System.Windows.Media.Pen(new SolidColorBrush(Colors.Black), 1.0);
				dc.DrawRectangle(
					null,
					sheetBorder,
					new Rect(ics.GetLocalPoint(new System.Windows.Point(0.0, 0.0), 1.0, new Vector(0.0, 0.0)), ics.GetLocalPoint(new System.Windows.Point(sheet.Length, sheet.Width), 1.0, new Vector(0.0, 0.0))));

				// Order geometry to draw aisle space first
				// Rect that drawn first will be on lowest layer on image
				BaseRectangleGeometry[] orderedRectangles = sheet.Rectangles.ToArray();
				Array.Sort(orderedRectangles, new RectanglesComparer());

				// draw geometry
				foreach (BaseRectangleGeometry geom in orderedRectangles)
				{
					if (geom == null)
						continue;

					// Dont draw SheetElevationGeometry, it is displayed only in the application
					if (geom is SheetElevationGeometry)
						continue;

					// draw pallets for the main racks
					Rack rackGeom = geom as Rack;
					if (rackGeom != null && bExportGeometryDimensions && sheet.Document != null && rackGeom.ShowPallet && rackGeom.IsFirstInRowColumn && rackGeom.Levels != null) 
					{
						RackLevel lastLevel = rackGeom.Levels.LastOrDefault();
						if (lastLevel != null)
						{
							geom.Draw(dc, ics, geomDisplaySettings);

							// draw last level
							_DrawPallets(dc, lastLevel, ics, geomDisplaySettings);
						}
					}
					else
						geom.Draw(dc, ics, geomDisplaySettings);

					// draw tie beams
					if (sheet.TieBeamsList != null)
					{
						foreach (TieBeam tieBeamGeom in sheet.TieBeamsList)
						{
							if (tieBeamGeom == null)
								continue;

							tieBeamGeom.Draw(dc, ics, geomDisplaySettings);
						}
					}

					//
                    if(bExportGeometryDimensions)
                    {
                        // export the size of the smallest side of AisleSpace
                        AisleSpace asGeom = geom as AisleSpace;
                        if(asGeom != null)
                        {
                            bool bIsXSmallest = false;
                            if (Utils.FLT(asGeom.Length_X, asGeom.Length_Y))
                                bIsXSmallest = true;

                            // display PTP-distance - it is distance without rack's overhang offset
                            bool bDisplayPTP = false;
                            bool bTopOvehang = false;
                            bool bBotOverhang = false;
                            bool bLeftOverhang = false;
                            bool bRightOverhang = false;
                            // DrawingDocument.RacksPalletType drives pallet types for all racks.
                            // If it is not overhang then all racks doesnt have overhang pallets.
                            if (sheet.Document.RacksPalletType == ePalletType.eOverhang)
                            {
                                // Find rack with overhang pallets connected to this AisleSpace.
                                // Overhang property is stored in the sheet and applied to all racks in the document.
                                // So check document for overhang property.
                                foreach (List<Rack> rackGroup in sheet.RacksGroups)
                                {
                                    if (rackGroup == null)
                                        continue;

                                    foreach (Rack rack in rackGroup)
                                    {
                                        if (rack == null)
                                            continue;

                                        if (bIsXSmallest)
                                        {
                                            // rack should be rotated
                                            if (rack.IsHorizontal)
                                                continue;

                                            // they should be intersected in Y-axis
                                            if (Utils.FLT(asGeom.BottomLeft_GlobalPoint.Y, rack.TopLeft_GlobalPoint.Y) || Utils.FGT(asGeom.TopLeft_GlobalPoint.Y, rack.BottomLeft_GlobalPoint.Y))
                                                continue;

                                            if (Utils.FEQ(rack.TopLeft_GlobalPoint.X, asGeom.TopRight_GlobalPoint.X))
                                            {
                                                bRightOverhang = true;
                                                break;
                                            }
                                            else if (Utils.FEQ(rack.TopRight_GlobalPoint.X, asGeom.TopLeft_GlobalPoint.X))
                                            {
                                                bLeftOverhang = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // rack should be not rotated
                                            if (!rack.IsHorizontal)
                                                continue;

                                            // skip if they are not intersected in X-axis
                                            if (Utils.FLT(asGeom.TopRight_GlobalPoint.X, rack.TopLeft_GlobalPoint.X) || Utils.FGT(asGeom.TopLeft_GlobalPoint.X, rack.TopRight_GlobalPoint.X))
                                                continue;

                                            if (Utils.FEQ(rack.TopLeft_GlobalPoint.Y, asGeom.BottomLeft_GlobalPoint.Y))
                                            {
                                                bBotOverhang = true;
                                                break;
                                            }
                                            else if (Utils.FEQ(rack.BottomLeft_GlobalPoint.Y, asGeom.TopLeft_GlobalPoint.Y))
                                            {
                                                bTopOvehang = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            //
                            if (bIsXSmallest && (bLeftOverhang || bRightOverhang))
                                bDisplayPTP = true;
                            else if (!bIsXSmallest && (bTopOvehang || bBotOverhang))
                                bDisplayPTP = true;

                            // display the smallest side length
                            System.Windows.Point pnt_01 = asGeom.BottomLeft_GlobalPoint;
                            System.Windows.Point pnt_02 = asGeom.TopLeft_GlobalPoint;
                            RackAdvancedPropertiesControl.eDimensionPlacement dimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eLeft;
                            string strDimText = asGeom.Length_Y.ToString();
                            if(bIsXSmallest)
                            {
                                pnt_01 = asGeom.TopLeft_GlobalPoint;
                                pnt_02 = asGeom.TopRight_GlobalPoint;
                                dimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eTop;
                                strDimText = asGeom.Length_X.ToString();
                            }

                            //
                            double dimPixelsOffest = m_DimensionOffsetInPixels;
                            // add text size
                            if (bDisplayPTP)
                                dimPixelsOffest += 1.5 * geomDisplaySettings.TextFontSize;

							dimensions.Add(new DimensionData(
								pnt_01, pnt_02, strDimText, dimPixelsOffest, ics.GetGlobalWidth(dimPixelsOffest, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0), 
								geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, dimPlacement));

                            // display PTP
                            if (bDisplayPTP)
                            {
                                double ptpDistValue = 0.0;
                                if (bIsXSmallest)
                                {
                                    ptpDistValue = asGeom.Length_X;
                                    // check left and right overhang
                                    if (bLeftOverhang)
                                    {
                                        pnt_01.X += sheet.Document.RacksPalletsOverhangValue;
                                        ptpDistValue -= sheet.Document.RacksPalletsOverhangValue;
                                    }
                                    if (bRightOverhang)
                                    {
                                        pnt_02.X -= sheet.Document.RacksPalletsOverhangValue;
                                        ptpDistValue -= sheet.Document.RacksPalletsOverhangValue;
                                    }
                                }
                                else
                                {
                                    ptpDistValue = asGeom.Length_Y;
                                    // check top and bot overhang
                                    if (bBotOverhang)
                                    {
                                        pnt_01.Y -= sheet.Document.RacksPalletsOverhangValue;
                                        ptpDistValue -= sheet.Document.RacksPalletsOverhangValue;
                                    }
                                    if (bTopOvehang)
                                    {
                                        pnt_02.Y += sheet.Document.RacksPalletsOverhangValue;
                                        ptpDistValue -= sheet.Document.RacksPalletsOverhangValue;
                                    }
                                }
                                strDimText = "PTP = " + ptpDistValue.ToString();

								dimensions.Add(new DimensionData(
									pnt_01, pnt_02, strDimText, m_DimensionOffsetInPixels, ics.GetGlobalWidth(m_DimensionOffsetInPixels, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0),
									geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, dimPlacement, isAisleSpace: true));
                            }
                        }
                    }
                }

				// draw sheet and rack groups dimensions
				if(bExportGeometryDimensions)
				{
					List<AisleSpace> aisleSpaceList = new List<AisleSpace>();
					foreach (BaseRectangleGeometry geom in sheet.Rectangles)
					{
						if (geom == null)
							continue;

						AisleSpace asGeom = geom as AisleSpace;
						if (asGeom != null)
						{
							aisleSpaceList.Add(asGeom);

							// text position is not accurate but it enough to detect collisions
							Rect approximatedTextposition = new Rect(
								ics.GetLocalPoint(new System.Windows.Point(asGeom.Center_GlobalPoint.X - 900, asGeom.Center_GlobalPoint.Y - 400), 1.0, new Vector(0.0, 0.0)),
								ics.GetLocalPoint(new System.Windows.Point(asGeom.Center_GlobalPoint.X + 900, asGeom.Center_GlobalPoint.Y + 400), 1.0, new Vector(0.0, 0.0))
								);

							textColisionlist.Add(approximatedTextposition);
						}
					}

					// export dimension for racks groups
					foreach (List<Rack> rackGroup in sheet.RacksGroups)
					{
						if (rackGroup == null)
							continue;

						if (rackGroup.Count == 0)
							continue;

						if(rackGroup[0] == null)
							continue;

						// rack's dimensions should be displayed at the side where is aisle space connected
						RackAdvancedPropertiesControl.eDimensionPlacement lengthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eTop;
						if (!rackGroup[0].IsHorizontal)
							lengthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eLeft;
						// go trought racks group and find connected rack
						foreach(Rack rack in rackGroup)
						{
							if (rack == null)
								continue;

							bool bAisleSpaceIsFound = false;
							foreach(AisleSpace aisleSpaceGeom in aisleSpaceList)
							{
								if (aisleSpaceGeom == null)
									continue;

								if(rack.IsHorizontal)
								{
									// skip if they are not intersected in X-axis
									if (Utils.FLT(aisleSpaceGeom.TopRight_GlobalPoint.X, rack.TopLeft_GlobalPoint.X) || Utils.FGT(aisleSpaceGeom.TopLeft_GlobalPoint.X, rack.TopRight_GlobalPoint.X))
										continue;
									
									if(Utils.FEQ(rack.TopLeft_GlobalPoint.Y, aisleSpaceGeom.BottomLeft_GlobalPoint.Y))
									{
										lengthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eTop;
										bAisleSpaceIsFound = true;
										break;
									}
									else if(Utils.FEQ(rack.BottomLeft_GlobalPoint.Y, aisleSpaceGeom.TopLeft_GlobalPoint.Y))
									{
										lengthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eBot;
										bAisleSpaceIsFound = true;
										break;
									}
								}
								else
								{
									// skip if they are not intersected in Y-axis
									if (Utils.FLT(aisleSpaceGeom.BottomLeft_GlobalPoint.Y, rack.TopLeft_GlobalPoint.Y) || Utils.FGT(aisleSpaceGeom.TopLeft_GlobalPoint.Y, rack.BottomLeft_GlobalPoint.Y))
										continue;

									if (Utils.FEQ(rack.TopLeft_GlobalPoint.X, aisleSpaceGeom.TopRight_GlobalPoint.X))
									{
										lengthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eLeft;
										bAisleSpaceIsFound = true;
										break;
									}
									else if (Utils.FEQ(rack.TopRight_GlobalPoint.X, aisleSpaceGeom.TopLeft_GlobalPoint.X))
									{
										lengthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eRight;
										bAisleSpaceIsFound = true;
										break;
									}
								}
							}

							if (bAisleSpaceIsFound)
								break;
						}

						// Export dimension only for unique rack indexes in the group.
						// List with rack indexes for which dimension was already exported.
						List<int> exportedDimensionsList = new List<int>();
						foreach(Rack rack in rackGroup)
						{
							if (rack == null)
								continue;

							if (exportedDimensionsList.Contains(rack.SizeIndex))
								continue;
                            exportedDimensionsList.Add(rack.SizeIndex);

                            // display length of the rack
                            System.Windows.Point pnt_01 = new System.Windows.Point();
							System.Windows.Point pnt_02 = new System.Windows.Point();
							string strDimText = rack.Length.ToString();
							if(RackAdvancedPropertiesControl.eDimensionPlacement.eLeft == lengthDimPlacement)
							{
								pnt_01 = rack.BottomLeft_GlobalPoint;
								pnt_02 = rack.TopLeft_GlobalPoint;
							}
							else if(RackAdvancedPropertiesControl.eDimensionPlacement.eRight == lengthDimPlacement)
							{
								pnt_01 = rack.BottomRight_GlobalPoint;
								pnt_02 = rack.TopRight_GlobalPoint;
							}
							else if (RackAdvancedPropertiesControl.eDimensionPlacement.eTop == lengthDimPlacement)
							{
								pnt_01 = rack.TopLeft_GlobalPoint;
								pnt_02 = rack.TopRight_GlobalPoint;
							}
							else if (RackAdvancedPropertiesControl.eDimensionPlacement.eBot == lengthDimPlacement)
							{
								pnt_01 = rack.BottomLeft_GlobalPoint;
								pnt_02 = rack.BottomRight_GlobalPoint;
							}
							// draw length dim

							dimensions.Add(new DimensionData(
								pnt_01, pnt_02, strDimText, m_DimensionOffsetInPixels, ics.GetGlobalWidth(m_DimensionOffsetInPixels, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0), 
								geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, lengthDimPlacement));

							// for the first rack in the group need to display its depth
							int iRackIndex = rackGroup.IndexOf(rack);
							if(iRackIndex == 0)
							{
								pnt_01 = rack.BottomLeft_GlobalPoint;
								pnt_02 = rack.TopLeft_GlobalPoint;
								RackAdvancedPropertiesControl.eDimensionPlacement depthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eLeft;
								strDimText = rack.Depth.ToString();
								if (!rack.IsHorizontal)
								{
									pnt_01 = rack.TopLeft_GlobalPoint;
									pnt_02 = rack.TopRight_GlobalPoint;
									depthDimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eTop;
								}
								
								dimensions.Add(new DimensionData(
									pnt_01, pnt_02, strDimText, m_DimensionOffsetInPixels, ics.GetGlobalWidth(m_DimensionOffsetInPixels, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0),
									geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, depthDimPlacement));
								
							}
						}

						// draw rack group length dimensions here
						var first = rackGroup[0];
						var last = rackGroup[rackGroup.Count - 1];
						
						double length = 0;
						System.Windows.Point startPoint = new System.Windows.Point();
						System.Windows.Point endPoint = new System.Windows.Point();

						double dimPixelsOffest = m_DimensionOffsetInPixels + 1.5 * geomDisplaySettings.TextFontSize;

						if (RackAdvancedPropertiesControl.eDimensionPlacement.eLeft == lengthDimPlacement)
						{
							startPoint = first.TopLeft_GlobalPoint;
							endPoint = last.BottomLeft_GlobalPoint;
						}
						else if (RackAdvancedPropertiesControl.eDimensionPlacement.eRight == lengthDimPlacement)
						{
							startPoint = first.TopRight_GlobalPoint;
							endPoint = last.BottomRight_GlobalPoint;
						}
						else if (RackAdvancedPropertiesControl.eDimensionPlacement.eTop == lengthDimPlacement)
						{
							startPoint = first.TopLeft_GlobalPoint;
							endPoint = last.TopRight_GlobalPoint;
						}
						else if (RackAdvancedPropertiesControl.eDimensionPlacement.eBot == lengthDimPlacement)
						{
							startPoint = first.BottomLeft_GlobalPoint;
							endPoint = last.BottomRight_GlobalPoint;
						}

						if (first.IsHorizontal)
						{
							length = Math.Abs(endPoint.X - startPoint.X);
						}
						else
						{
							length = Math.Abs(endPoint.Y - startPoint.Y);
						}

						DimensionData rackGroupDim;

						if (first.IsHorizontal)
                        {
							rackGroupDim = new DimensionData(
								startPoint, endPoint, length.ToString(), dimPixelsOffest, ics.GetGlobalWidth(dimPixelsOffest, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0),
								geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, lengthDimPlacement, true);
						}
                        else
                        {
							rackGroupDim = new DimensionData(
								endPoint, startPoint, length.ToString(), dimPixelsOffest, ics.GetGlobalWidth(dimPixelsOffest, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0),
								geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, lengthDimPlacement, true);
						}
						

						if (dimensions.All(x => x.Value != rackGroupDim.Value))
							dimensions.Add(rackGroupDim);

					}

					// sheet can have rack placed at (0,0), so rack's dim will be displayed at the top and overlap sheet's length dim
					double sheetDimOffset = m_DimensionOffsetInPixels;
					sheetDimOffset += 1.5 * geomDisplaySettings.TextFontSize;

					dimensions.Add(new DimensionData(
								new System.Windows.Point(0, 0), new System.Windows.Point(sheet.Length, 0), sheet.Length.ToString(),
								sheetDimOffset, ics.GetGlobalWidth(sheetDimOffset, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0),
								geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, RackAdvancedPropertiesControl.eDimensionPlacement.eTop, true));

					dimensions.Add(new DimensionData(
								new System.Windows.Point(sheet.Length, sheet.Width), new System.Windows.Point(sheet.Length, 0.0), sheet.Width.ToString(),
								sheetDimOffset, ics.GetGlobalWidth(sheetDimOffset, 0.0) + ics.GetGlobalWidth(geomDisplaySettings.TextFontSize / 4, 0.0),
								geomDisplaySettings.TextFontSize, geomDisplaySettings.TextFontSize / 4, RackAdvancedPropertiesControl.eDimensionPlacement.eRight, true));

					// sheet length dimension
					//RackAdvancedPropertiesControl._DrawDimension(
					//	dc,
					//	new System.Windows.Point(0, 0),
					//	new System.Windows.Point(sheet.Length, 0),
					//	sheet.Length.ToString(),
					//	sheetDimOffset,
					//	geomDisplaySettings.TextFontSize,
					//	geomDisplaySettings.TextFontSize / 4,
					//	RackAdvancedPropertiesControl.eDimensionPlacement.eTop,
					//	ics
					//	);

					//// sheet height dimension
					//RackAdvancedPropertiesControl._DrawDimension(
					//	dc,
					//	new System.Windows.Point(sheet.Length, sheet.Width),
					//	new System.Windows.Point(sheet.Length, 0.0),
					//	sheet.Width.ToString(),
					//	sheetDimOffset,
					//	geomDisplaySettings.TextFontSize,
					//	geomDisplaySettings.TextFontSize / 4,
					//	RackAdvancedPropertiesControl.eDimensionPlacement.eRight,
					//	ics
					//	);

					//TODO: check overlapping
					double dimensionOffset = 2000.0;

					foreach (var dimension in dimensions.Where(x => x.IsRackGroup))
					{
						Rect dimRect = new Rect(
							ics.GetLocalPoint(dimension.TextColisionRect.TopLeft, 1.0, new Vector(0.0, 0.0)),
							ics.GetLocalPoint(dimension.TextColisionRect.BottomRight, 1.0, new Vector(0.0, 0.0)));

						//dc.DrawRectangle(new SolidColorBrush(Colors.Red), null, dimRect);

                        foreach (Rect text in textColisionlist)
                        {
							if (dimension.IsRackGroup && text.IntersectsWith(dimRect))
							{
								dimension.DimensionTextColisionOffset = dimensionOffset;
							}
                            //TODO calc offset
						}

						foreach (var item in dimensions.Where(x => !x.IsRackGroup))
						{
							if (dimension.IsAisleSpace && item.IsAisleSpace)
								continue;

							Rect otherDimRect = new Rect(
								ics.GetLocalPoint(item.TextColisionRect.TopLeft, 1.0, new Vector(0.0, 0.0)),
								ics.GetLocalPoint(item.TextColisionRect.BottomRight, 1.0, new Vector(0.0, 0.0)));

							if (otherDimRect.IntersectsWith(dimRect))
							{
								dimension.DimensionTextColisionOffset += dimensionOffset;
							}
						}
                    }
					
					foreach (DimensionData dimension in dimensions)
                    {
                        RackAdvancedPropertiesControl._DrawDimension(
                            dc,
                            dimension.First,
                            dimension.Secont,
                            dimension.DimensionText,
                            dimension.DimensionPixelsOffset,
							dimension.TextSize,
                            dimension.PerpDimLineOffsetInPixels,
                            dimension.DimensionPlacement,
                            ics,
							dimensionTextOffset: dimension.DimensionTextColisionOffset
							);
                    }
                }

				// Draw horizontal and vertical sheet elevations
				if(bIncludeSheetElevationsPictures)
				{
					if (horizontalSheetElevationRacksList != null && horizontalSheetElevationRacksList.Count > 0)
					{
						System.Windows.Point horizSheetElevationStartPoint = ics.GetLocalPoint(new System.Windows.Point(0.0, sheet.Width), 1.0, new Vector(0.0, 0.0));
						horizSheetElevationStartPoint.Y += m_SheetElevationsOffsetInPixels;
						horizSheetElevationStartPoint.Y += ics.GetHeightInPixels(horizontalSheetElevation_BiggestRackHeight, 1.0);
						// offsetInPixels is already included in ics(Coordinate System), so remove it, otherwise DrawSheetElevation will include it twice
						horizSheetElevationStartPoint -= ics.OffsetInPixels;
						DrawSheetElevation(dc, ics, horizontalSheetElevationRacksList, horizSheetElevationStartPoint, m_DimensionOffsetInPixels, true, geomDisplaySettings.TextFontSize, false);
					}

					if(verticalSheetElevationRacksList != null && verticalSheetElevationRacksList.Count > 0)
					{
						//System.Windows.Point vertSheetElevationStartPoint = ics.GetLocalPoint(new System.Windows.Point(sheet.Length, sheet.Width), 1.0, new Vector(0.0, 0.0));
						System.Windows.Point vertSheetElevationStartPoint = ics.GetLocalPoint(new System.Windows.Point(sheet.Length, 0.0), 1.0, new Vector(0.0, 0.0));
						vertSheetElevationStartPoint.X += m_SheetElevationsOffsetInPixels;
						vertSheetElevationStartPoint.X += ics.GetHeightInPixels(verticalSheetElevation_BiggestRackHeight, 1.0);
						// offsetInPixels is already included in ics(Coordinate System), so remove it, otherwise DrawSheetElevation will include it twice
						vertSheetElevationStartPoint -= ics.OffsetInPixels;
						DrawSheetElevation(dc, ics, verticalSheetElevationRacksList, vertSheetElevationStartPoint, m_DimensionOffsetInPixels, false, geomDisplaySettings.TextFontSize, false);
					}
				}

				// draw watermark
				if (watermarkImage != null)
					WatermarkVisual.sDrawWatermark(dc, imageLength, imageHeight, watermarkImage);
			}

			return visual;
		}

		private class DimensionData
		{
            public bool IsRackGroup { get; private set; }
            public bool IsAisleSpace { get; private set; }
            public System.Windows.Point First { get; private set; }
			public System.Windows.Point Secont { get; private set; }
			public string DimensionText { get; private set; }
			public double DimensionPixelsOffset { get; set; }
			public double DimensionTextOffset { get; set; }
			public double TextSize { get; set; }
            public double PerpDimLineOffsetInPixels { get; set; }
			public RackAdvancedPropertiesControl.eDimensionPlacement DimensionPlacement { get; private set; }
			public double Value
			{
				get
				{
					double result;

					if (DimensionPlacement == RackAdvancedPropertiesControl.eDimensionPlacement.eBot || DimensionPlacement == RackAdvancedPropertiesControl.eDimensionPlacement.eTop)
						result = Secont.X - First.X;
					else
						result = Secont.Y - First.Y;

					return Math.Abs(result);
				}
			}

			public double DimensionTextColisionOffset { get; set; } = 0.0;

            public Rect TextColisionRect { get; private set; }

            public DimensionData(
				System.Windows.Point first,
				System.Windows.Point second,
				string dimensionText,
				double dimensionPixelsOffset,
				double dimensionTextOffset,
				double textSize,
				double perpDimLineOffsetInPixels,
				RackAdvancedPropertiesControl.eDimensionPlacement dimensionPlacement,
				bool isRackGroup = false,
				bool isAisleSpace = false)
            {
				First = first;
				Secont = second;
				DimensionText = dimensionText;
				DimensionPixelsOffset = dimensionPixelsOffset;
				DimensionTextOffset = dimensionTextOffset;
				DimensionPlacement = dimensionPlacement;
				IsRackGroup = isRackGroup;
				IsAisleSpace = isAisleSpace;
				TextSize = textSize;
				PerpDimLineOffsetInPixels = perpDimLineOffsetInPixels;

				System.Windows.Point left;
				System.Windows.Point right;

				switch (dimensionPlacement)
                {
                    case RackAdvancedPropertiesControl.eDimensionPlacement.eBot:
                        left = new System.Windows.Point(first.X + (Value / 2) - 800, first.Y + DimensionTextOffset);
                        right = new System.Windows.Point(first.X + (Value / 2) + 800, left.Y + 400);
						TextColisionRect = new Rect(left, right);

						break;
                    case RackAdvancedPropertiesControl.eDimensionPlacement.eTop:
						left = new System.Windows.Point(first.X + (Value / 2) - 800, first.Y - DimensionTextOffset);
						right = new System.Windows.Point(first.X + (Value / 2) + 800, left.Y - 400);
						TextColisionRect = new Rect(left, right);

						break;
                    case RackAdvancedPropertiesControl.eDimensionPlacement.eLeft:
						left = new System.Windows.Point(first.X - DimensionTextOffset, first.Y - (Value / 2) - 800);
						right = new System.Windows.Point(second.X - DimensionTextOffset - 400, second.Y + (Value / 2) + 800);
						TextColisionRect = new Rect(left, right);

						break;
                    case RackAdvancedPropertiesControl.eDimensionPlacement.eRight:
						left = new System.Windows.Point(first.X + DimensionTextOffset, first.Y - (Value / 2) - 800);
						right = new System.Windows.Point(second.X + DimensionTextOffset - 400, second.Y + (Value / 2) + 800);
						TextColisionRect = new Rect(left, right);

						break;
                }
            }
        }

		public static DrawingVisual CreateSheetElevationVisual(
			DrawingSheet sheet,
			ImageSource watermarkImage,
			int imageLength,
			int imageHeight,
			bool bIsItHorizontalSheetElevation,
			List<Rack> sheetElevationRacksList,
			double biggestRackHeight,
			double minUnitsPerPixel = 0.0,
			bool ignoreRackGlobalPosition = false)
		{
			if (sheet == null)
				return null;

			if (sheet.Document == null)
				return null;

			if (sheet.Length == 0 || sheet.Width == 0)
				return null;

			if (sheetElevationRacksList == null)
				return null;
			if (sheetElevationRacksList.Count == 0)
				return null;

			if (Utils.FLE(biggestRackHeight, 0.0))
				return null;

			double exportFontSize = ExportFontSizeDependencies.GetFontSize(imageLength, imageHeight);

			//
			GC.Collect();
			GC.WaitForPendingFinalizers();

			IGeomDisplaySettings geomDisplaySettings = DefaultGeomDisplaySettings.GetInstance();
			geomDisplaySettings.TextFontSize = exportFontSize;

			//
			int totalImageLength = imageLength;
			int totalImageHeight = imageHeight;
			// left and right extra space
			totalImageLength -= 2 * m_ExtraSpaceForDimension;
			// top and bot extra space
			totalImageHeight -= 2 * m_ExtraSpaceForDimension;
			Vector offsetInPixels = new Vector(m_ExtraSpaceForDimension, m_ExtraSpaceForDimension);

			System.Windows.Size drawingSize = new System.Windows.Size(sheet.Length, sheet.Width);
			if (bIsItHorizontalSheetElevation)
				drawingSize.Height = biggestRackHeight;
			else
				drawingSize.Width = biggestRackHeight;


			if (ignoreRackGlobalPosition)
			{
				double sizeScale = 1.0;

				if (!bIsItHorizontalSheetElevation)
					sizeScale = (double)totalImageLength / sheet.Length;
				else
					sizeScale = (double)totalImageHeight / sheet.Width;

				if (!bIsItHorizontalSheetElevation)
					totalImageLength = (int)Math.Ceiling(sizeScale * drawingSize.Width);
                else
					totalImageHeight = (int)Math.Ceiling(sizeScale * drawingSize.Height);
			}

			ImageCoordinateSystem ics = new ImageCoordinateSystem(totalImageLength, totalImageHeight, offsetInPixels, drawingSize, minUnitsPerPixel);

            //
            DrawingVisual visual = new DrawingVisual();
			using (DrawingContext dc = visual.RenderOpen())
			{
				// draw background
				dc.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(new System.Windows.Point(0.0, 0.0), new System.Windows.Point(imageLength, imageHeight)));
				//// draw sheet border
				//System.Windows.Media.Pen sheetBorder = null;
				////if (bSizeMode)
				//sheetBorder = new System.Windows.Media.Pen(new SolidColorBrush(Colors.Black), 1.0);
				//dc.DrawRectangle(
				//	null,
				//	sheetBorder,
				//	new Rect(ics.GetLocalPoint(new System.Windows.Point(0.0, 0.0), 1.0, new Vector(0.0, 0.0)), ics.GetLocalPoint(new System.Windows.Point(sheet.Length, sheet.Width), 1.0, new Vector(0.0, 0.0))));

				// Draw horizontal and vertical sheet elevations
				if (bIsItHorizontalSheetElevation)
				{
					System.Windows.Point horizSheetElevationStartPoint = ics.GetLocalPoint(new System.Windows.Point(0.0, 0.0), 1.0, new Vector(0.0, 0.0));
					horizSheetElevationStartPoint.Y += ics.GetHeightInPixels(biggestRackHeight, 1.0);
					// offsetInPixels is already included in ics(Coordinate System), so remove it, otherwise DrawSheetElevation will include it twice
					horizSheetElevationStartPoint -= ics.OffsetInPixels;
					DrawSheetElevation(dc, ics, sheetElevationRacksList, horizSheetElevationStartPoint, m_DimensionOffsetInPixels, true, geomDisplaySettings.TextFontSize, false, ignoreRackGlobalPosition);
				}
				else
				{
					//System.Windows.Point vertSheetElevationStartPoint = ics.GetLocalPoint(new System.Windows.Point(sheet.Length, sheet.Width), 1.0, new Vector(0.0, 0.0));
					System.Windows.Point vertSheetElevationStartPoint = ics.GetLocalPoint(new System.Windows.Point(0.0, 0.0), 1.0, new Vector(0.0, 0.0));
					vertSheetElevationStartPoint.X += ics.GetHeightInPixels(biggestRackHeight, 1.0);
					// offsetInPixels is already included in ics(Coordinate System), so remove it, otherwise DrawSheetElevation will include it twice
					vertSheetElevationStartPoint -= ics.OffsetInPixels;
					DrawSheetElevation(dc, ics, sheetElevationRacksList, vertSheetElevationStartPoint, m_DimensionOffsetInPixels, false, geomDisplaySettings.TextFontSize, false, ignoreRackGlobalPosition);
				}

				// draw watermark
				if (watermarkImage != null)
					WatermarkVisual.sDrawWatermark(dc, imageLength, imageHeight, watermarkImage);
			}

			return visual;
		}

		/// <summary>
		/// Draw sheet elevation picture.
		/// </summary>
		/// <param name="sheetElevationsRacksList">
		/// Racks which should be displayed at sheet elevation picture
		/// </param>
		/// <param name="startPointInPixels">
		/// Picture point(in pixels) at which sheet elevation picture should be placed.
		/// For horizontal sheet elevation it is bot left sheet elevation point.
		/// For vertical sheet elevation it is top right sheet elevation point.
		/// </param>
		/// <param name="bDrawHorizontal">
		/// If true then need to draw HORIZONTAL sheet elevation picture, if false - VERTICAL.
		/// </param>
		/// <param name="exportToWarehouseSheet">
		/// If true, then need to apply addtional offset.
		/// Because sheet is placed somewhere at Warehouse sheet.
		/// </param>
		private static void DrawSheetElevation(
			DrawingContext dc,
			ICoordinateSystem cs,
			List<Rack> sheetElevationsRacksList,
			System.Windows.Point startPointInPixels,
			double firstDimLineOffset,
			bool bDrawHorizontal,
			double dimFontSizeInPixels,
			bool exportToWarehouseSheet = false,
			bool isPullRacksToLeft = false)
		{
			if (dc == null)
				return;
			if (cs == null)
				return;
			if (sheetElevationsRacksList == null)
				return;

			RackAdvancedDrawingSettings sheetElevationDisplaySettings = RackAdvancedDrawingSettings.GetSheetElevationDefaultSettings();
			Rack firstRack = sheetElevationsRacksList.FirstOrDefault();
			Rack lastRack = sheetElevationsRacksList.LastOrDefault();

			// Set of racks, for which front view length dimension is already displayed
			HashSet<Rack> frontView_LengthDimDisplayed_RacksSet = new HashSet<Rack>();

			double rackPositionToLeftOffset = 0.0;
			double rackPositionToTopOffset = 0.0;
			isPullRacksToLeft = false;
			if (isPullRacksToLeft)
			{
				double das = RackUtils.GetTheBiggestRackHeight(sheetElevationsRacksList);
				rackPositionToLeftOffset = firstRack.BottomLeft_GlobalPoint.X - 150;//cs.GetGlobalWidth(150, 0.0);
				rackPositionToTopOffset = firstRack.BottomLeft_GlobalPoint.Y - das;//cs.GetGlobalWidth(das, 0.0);
			}

			System.Windows.Point sheetTopLeft_PicturePoint = cs.GetLocalPoint(new System.Windows.Point(0.0, 0.0), 1.0, new Vector(0.0, 0.0));
			foreach (Rack rack in sheetElevationsRacksList)
			{
				if (rack == null || rack.Sheet == null)
					continue;

				// Draw front or side view of the rack
				bool bDrawFrontView = true;
				if ((bDrawHorizontal && !rack.IsHorizontal) || (!bDrawHorizontal && rack.IsHorizontal))
					bDrawFrontView = false;

				// By default rack front view for A-rack doesnt have left column.
				// It is ok for horizontal sheet elevation picture.
				// But for vertical sheet elevation picture -90 degrees transform is applied.
				// It means that front view of the rack for vertical sheet elevation doesnt have column at the bottom.
				// It is not correct, it should not have column at the top side.
				// Need to mirror front veiw in this case.
				bool bMirrorFrontView = !bDrawHorizontal && bDrawFrontView;

				System.Windows.Point rackBotLeftGlobalPnt = rack.BottomLeft_GlobalPoint;
				rackBotLeftGlobalPnt.X -= rackPositionToLeftOffset;
				rackBotLeftGlobalPnt.Y -= rackPositionToTopOffset;

				//if (exportToWarehouseSheet)
				//	rackBotLeftGlobalPnt = GetRackPointRelativeToWarehouseSheet(rack, rack.BottomLeft_GlobalPoint);

				System.Windows.Point rackBotLeft_PicturePnt = cs.GetLocalPoint(rackBotLeftGlobalPnt, 1.0, new Vector(0.0, 0.0));
				Vector viewOffsetInPixels = startPointInPixels - new System.Windows.Point(0.0, 0.0);
				if (bDrawHorizontal)
				{
					viewOffsetInPixels.X += rackBotLeft_PicturePnt.X - sheetTopLeft_PicturePoint.X;
					// include pallet overhang value for side view
					if (!bDrawFrontView)
						viewOffsetInPixels.X -= cs.GetWidthInPixels(rack.PalletOverhangValue, 1.0);
				}
				else
				{
					viewOffsetInPixels.Y += rackBotLeft_PicturePnt.Y - sheetTopLeft_PicturePoint.Y;
					// include pallet overhang value for side view
					if (!bDrawFrontView)
						viewOffsetInPixels.Y += cs.GetHeightInPixels(rack.PalletOverhangValue, 1.0);
				}

				int iTransformCount = 0;
				// DrawRackFrontView() and DrawRackSideView() draws view at (0.0, 0.0) point.
				// So, need to apply offset to the DrawingContext.
				dc.PushTransform(new TranslateTransform(viewOffsetInPixels.X, viewOffsetInPixels.Y));
				++iTransformCount;
				// Apply rotate for vertical sheet elevation picture
				if (!bDrawHorizontal)
				{
					dc.PushTransform(new RotateTransform(-90, sheetTopLeft_PicturePoint.X, sheetTopLeft_PicturePoint.Y));
					++iTransformCount;
				}

				if (bMirrorFrontView)
				{
					double rackLengthInPixels = cs.GetWidthInPixels(rack.Length, 1.0);
					dc.PushTransform(new ScaleTransform(-1.0, 1.0, sheetTopLeft_PicturePoint.X + rackLengthInPixels / 2, 0.0));
					++iTransformCount;
				}

				if (bDrawFrontView)
					RackAdvancedPropertiesControl.DrawRackFrontView(dc, cs, sheetElevationDisplaySettings, rack, null);
				else
					RackAdvancedPropertiesControl.DrawRackSideView(dc, cs, sheetElevationDisplaySettings, rack, null);

				// Remove front view mirror before draw dimensions
				if (bMirrorFrontView)
				{
					dc.Pop();
					--iTransformCount;
				}

				double perLineOffset = dimFontSizeInPixels / 4;
				double suppLineOffsetValue = dimFontSizeInPixels + 2 * perLineOffset;

				// Draw rack underpass dimension
				if (bDrawFrontView && rack.IsUnderpassAvailable)
				{
					double underpassValue = rack.Underpass;
					System.Windows.Point dimPnt_01 = new System.Windows.Point(rack.Length / 2, 0.0);
					System.Windows.Point dimPnt_02 = new System.Windows.Point(dimPnt_01.X, -underpassValue);

					double textRotateAngleDegrees = 0.0;
					bool bMirrorDimTextRelativeDimLine = false;
					if (!bDrawHorizontal)
					{
						textRotateAngleDegrees = -180;
						bMirrorDimTextRelativeDimLine = true;
					}

					RackAdvancedPropertiesControl._DrawDimension(
						dc,
						dimPnt_01,
						dimPnt_02,
						"UP=" + underpassValue.ToString(),
						0.0,
						dimFontSizeInPixels,
						perLineOffset,
						RackAdvancedPropertiesControl.eDimensionPlacement.eLeft,
						cs,
						textRotateAngleDegrees: textRotateAngleDegrees,
						bMirrorTextRelativeToDimLine: bMirrorDimTextRelativeDimLine);
				}

				// For side view draw depth dimension
				if (!bDrawFrontView)
				{
					double rackDepth = rack.Depth;
					System.Windows.Point dimPnt_01 = new System.Windows.Point(rack.PalletOverhangValue, 0.0);
					System.Windows.Point dimPnt_02 = new System.Windows.Point(dimPnt_01.X + rackDepth, 0.0);

					RackAdvancedPropertiesControl._DrawDimension(
						dc,
						dimPnt_01,
						dimPnt_02,
						rackDepth.ToString(),
						suppLineOffsetValue,
						dimFontSizeInPixels,
						perLineOffset,
						RackAdvancedPropertiesControl.eDimensionPlacement.eBot,
						cs,
						bMirrorTextRelativeToDimLine: true);
				}

				// Draw MaxMaterialHeight and Height dimensions for the first and last rack.
				// Dont export last rack dimensions if:
				// 1. Last rack is equal to the first rack
				// 2. Last rack MaterialHeight and Height are equal to first rack
				if (firstRack == rack || (lastRack == rack && firstRack != lastRack && (Utils.FNE(lastRack.MaterialHeight, firstRack.MaterialHeight) || Utils.FNE(lastRack.Length_Z, firstRack.Length_Z))))
				{
					RackAdvancedPropertiesControl.eDimensionPlacement dimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eLeft;
					// firstRack != lastRack
					// means it is single rack in sheet elevation, display dimensions at the left side
					if (lastRack == rack && firstRack != lastRack)
						dimPlacement = RackAdvancedPropertiesControl.eDimensionPlacement.eRight;

					List<double> dimValuesList = new List<double>();
					dimValuesList.Add(rack.MaterialHeight);
					dimValuesList.Add(rack.Length_Z);
					dimValuesList.Sort();

					double suppLinesOffset = firstDimLineOffset;
					// In this case(last rack at horiz sheet elevation or first rack at vert sheet elevation)
					// then dim text is placed between dim line and rack.
					// firstDimLineOffset is too small in this case.
					if ((bDrawHorizontal && rack == lastRack) || (!bDrawHorizontal && rack == firstRack))
						suppLinesOffset = suppLineOffsetValue;
					double textRotateAngleDegrees = 0.0;
					bool bMirrorDimTextRelativeDimLine = false;
					if (!bDrawHorizontal)
					{
						textRotateAngleDegrees = -180;
						bMirrorDimTextRelativeDimLine = true;
					}
					foreach (double dimValue in dimValuesList)
					{
						System.Windows.Point dimPnt_01 = new System.Windows.Point(0.0, 0.0);
						if (lastRack == rack && firstRack != lastRack)
						{
							if (bDrawFrontView)
								dimPnt_01.X += rack.Length;
							else
								dimPnt_01.X += rack.Depth + 2 * rack.PalletOverhangValue;
						}
						System.Windows.Point dimPnt_02 = new System.Windows.Point(dimPnt_01.X, -dimValue);

						RackAdvancedPropertiesControl._DrawDimension(
							dc,
							dimPnt_01,
							dimPnt_02,
							dimValue.ToString(),
							suppLinesOffset,
							dimFontSizeInPixels,
							perLineOffset,
							dimPlacement,
							cs,
							textRotateAngleDegrees: textRotateAngleDegrees,
							bMirrorTextRelativeToDimLine: bMirrorDimTextRelativeDimLine);

						suppLinesOffset += suppLineOffsetValue;
					}
				}

				// Draw length dimension for front view racks.
				// 1. If racks are in the same group(row\column) then length dimension should be displayed for entire rack's group
				// 2. If racks group contains rack with underpass, then need to split 1 length dimension to 3 dimensions:
				//    2.1 From group start to the end of rack without underpass
				//    2.2 Underpass rack length without column, length of underpass hole
				//    2.3 From start of underpass rack right column to the end of the rack group or next underpass rack
				if (bDrawFrontView && !frontView_LengthDimDisplayed_RacksSet.Contains(rack))
				{
					frontView_LengthDimDisplayed_RacksSet.Add(rack);

					System.Windows.Point dimPnt_01 = new System.Windows.Point(0.0, 0.0);
					System.Windows.Point dimPnt_02 = new System.Windows.Point(rack.Length, 0.0);

					int iCurRackIndex = sheetElevationsRacksList.IndexOf(rack);
					for(int iRackIndex=iCurRackIndex + 1; iRackIndex < sheetElevationsRacksList.Count; ++iRackIndex)
					{
						Rack nextRack = sheetElevationsRacksList[iRackIndex];
						if (nextRack == null)
							break;

						// Next racks group.
						// Draw prev rack group length dimension.
						if (bDrawHorizontal && nextRack.IsFirstInRowColumn)
							break;

						// For vertical sheet elevation picture racks are displayed from the end(A-rack) of the group to the start(M-rack).
						// If current rack is M-rack, then group ends.
						if (!bDrawHorizontal && rack.IsFirstInRowColumn)
							break;

						bool bDrawFrontView_NextRack = true;
						if ((bDrawHorizontal && !nextRack.IsHorizontal) || (!bDrawHorizontal && nextRack.IsHorizontal))
							bDrawFrontView_NextRack = false;
						if (!bDrawFrontView_NextRack)
							break;

						if (nextRack.IsUnderpassAvailable)
						{
							frontView_LengthDimDisplayed_RacksSet.Add(nextRack);

							// For vertical sheet elevation picture rack's group starts with the last one(A-rack) and ends with the first M-rack.
							// Need to add next column length
							if (!bDrawHorizontal && nextRack.Column != null)
								dimPnt_02.X += nextRack.Column.Length;

							// Draw length dimension before underpass
							RackAdvancedPropertiesControl._DrawDimension(
								dc,
								dimPnt_01,
								dimPnt_02,
								(dimPnt_02.X - dimPnt_01.X).ToString("0."),
								suppLineOffsetValue,
								dimFontSizeInPixels,
								perLineOffset,
								RackAdvancedPropertiesControl.eDimensionPlacement.eBot,
								cs,
								bMirrorTextRelativeToDimLine: true);

							// Draw underpass hole length dimension
							dimPnt_01.X = dimPnt_02.X;
							dimPnt_02.X = dimPnt_01.X + nextRack.InnerLength;
							RackAdvancedPropertiesControl._DrawDimension(
								dc,
								dimPnt_01,
								dimPnt_02,
								(dimPnt_02.X - dimPnt_01.X).ToString("0."),
								suppLineOffsetValue,
								dimFontSizeInPixels,
								perLineOffset,
								RackAdvancedPropertiesControl.eDimensionPlacement.eBot,
								cs,
								bMirrorTextRelativeToDimLine: true);

							// Continue length dimension from the start of underpass rack column
							dimPnt_01.X = dimPnt_02.X;
							dimPnt_02.X = dimPnt_01.X;
							if (bDrawHorizontal && nextRack.Column != null)
								dimPnt_02.X += nextRack.Column.Length;

							continue;
						}

						frontView_LengthDimDisplayed_RacksSet.Add(nextRack);
						dimPnt_02.X += nextRack.Length;

						// For vertical sheet elevation picture rack's group starts with the last one(A-rack) and ends with the first M-rack.
						if (!bDrawHorizontal && nextRack.IsFirstInRowColumn)
							break;
					}

					RackAdvancedPropertiesControl._DrawDimension(
						dc,
						dimPnt_01,
						dimPnt_02,
						(dimPnt_02.X - dimPnt_01.X).ToString("0."),
						suppLineOffsetValue,
						dimFontSizeInPixels,
						perLineOffset,
						RackAdvancedPropertiesControl.eDimensionPlacement.eBot,
						cs,
						bMirrorTextRelativeToDimLine: true);
				}

				// Need to display distance dimension between the columns of different groups racks.
				if (true)
				{
					Rack nextRack = null;
					int iNextRackIndex = sheetElevationsRacksList.IndexOf(rack) + 1;
					if (iNextRackIndex < sheetElevationsRacksList.Count)
						nextRack = sheetElevationsRacksList[iNextRackIndex];
					if(nextRack != null && rack.Sheet != null)
					{
						List<Rack> currRackGroup = rack.Sheet.GetRackGroup(rack);
						if(currRackGroup != null && !currRackGroup.Contains(nextRack))
						{
							double distanceBetweenColumns = 0.0;
							if (bDrawHorizontal)
							{
								//if (exportToWarehouseSheet)
								//	distanceBetweenColumns = GetRackPointRelativeToWarehouseSheet(nextRack, nextRack.BottomLeft_GlobalPoint).X - GetRackPointRelativeToWarehouseSheet(rack, rack.BottomRight_GlobalPoint).X;
								//else

								distanceBetweenColumns = nextRack.BottomLeft_GlobalPoint.X - rack.BottomRight_GlobalPoint.X;
							}
							else
							{
								//if (exportToWarehouseSheet)
								//	distanceBetweenColumns = GetRackPointRelativeToWarehouseSheet(rack, rack.TopRight_GlobalPoint).Y - GetRackPointRelativeToWarehouseSheet(nextRack, nextRack.BottomRight_GlobalPoint).Y;
								//else

								distanceBetweenColumns = rack.TopRight_GlobalPoint.Y - nextRack.BottomRight_GlobalPoint.Y;
							}

							System.Windows.Point dimPnt_01 = new System.Windows.Point(0.0, 0.0);
							if (bDrawFrontView)
								dimPnt_01.X = rack.Length;
							else
								dimPnt_01.X = rack.PalletOverhangValue + rack.Depth;
							System.Windows.Point dimPnt_02 = dimPnt_01;
							dimPnt_02.X += distanceBetweenColumns;

							RackAdvancedPropertiesControl._DrawDimension(
								dc,
								dimPnt_01,
								dimPnt_02,
								distanceBetweenColumns.ToString("0."),
								suppLineOffsetValue,
								dimFontSizeInPixels,
								perLineOffset,
								RackAdvancedPropertiesControl.eDimensionPlacement.eBot,
								cs,
								bMirrorTextRelativeToDimLine: false);
						}
					}
				}

				// Pop all transforms
				for (int iTransformIndex = 0; iTransformIndex < iTransformCount; ++iTransformIndex)
					dc.Pop();
			}
		}
		private static void _DrawPallets(DrawingContext dc, RackLevel level, ICoordinateSystem cs, IGeomDisplaySettings geomDisplaySettings)
		{
			if (dc == null)
				return;

			if (level == null || level.Owner == null)
				return;

			if (level.Pallets == null || level.Pallets.Count == 0)
				return;

			if (cs == null)
				return;

			if (geomDisplaySettings == null)
				return;

			System.Windows.Media.Brush palletBorderBrush = CurrentGeometryColorsTheme.GetRackAdvProps_PalletBorderBrush();
			System.Windows.Media.Pen pen = new System.Windows.Media.Pen(palletBorderBrush, 3.0);

			double DistanceBetweenPallets = level.Owner.BeamLength;
			foreach (Pallet pallet in level.Pallets)
			{
				if (pallet == null)
					continue;

				DistanceBetweenPallets -= pallet.Length;
			}
			DistanceBetweenPallets /= (level.Pallets.Count + 1);

			Vector direction = new Vector(1.0, 0.0);
			if (!level.Owner.IsHorizontal)
				direction = new Vector(0.0, 1.0);

			System.Windows.Point startPnt = level.Owner.TopLeft_GlobalPoint;
			if(level.Owner.IsHorizontal)
				startPnt = new System.Windows.Point(startPnt.X, startPnt.Y + level.Owner.Depth / 2);
			else
				startPnt = new System.Windows.Point(startPnt.X + level.Owner.Depth / 2, startPnt.Y);

			double PalletOffset = DistanceBetweenPallets + Rack.INNER_LENGTH_ADDITIONAL_GAP / 2;
			if(level.Owner.Column != null)
				PalletOffset += level.Owner.Column.Length;

			// camera scale and offset are used for calculate screen(control) point based on global drawing point
			double defaultCameraScale = 1.0;
			Vector defaultCameraOffset = new Vector(0.0, 0.0);
			foreach (Pallet pallet in level.Pallets)
			{
				if (pallet == null)
					continue;

				double pallet_X_dim = pallet.Length;
				double pallet_Y_dim = pallet.Width;
				if(!level.Owner.IsHorizontal)
				{
					pallet_X_dim = pallet_Y_dim;
					pallet_Y_dim = pallet.Length;
				}

				System.Windows.Point palletCenter = startPnt + (PalletOffset + pallet.Length / 2) * direction;
				//
				System.Windows.Point pnt_01 = palletCenter;
				pnt_01.X -= pallet_X_dim / 2;
				pnt_01.Y -= pallet_Y_dim / 2;
				//
				System.Windows.Point pnt_02 = pnt_01;
				pnt_02.X += pallet_X_dim;
				//
				System.Windows.Point pnt_03 = pnt_02;
				pnt_03.Y += pallet_Y_dim;
				//
				System.Windows.Point pnt_04 = pnt_03;
				pnt_04.X = pnt_01.X;

				//
				System.Windows.Point localPnt_01 = cs.GetLocalPoint(pnt_01, defaultCameraScale, defaultCameraOffset);
				System.Windows.Point localPnt_02 = cs.GetLocalPoint(pnt_02, defaultCameraScale, defaultCameraOffset);
				System.Windows.Point localPnt_03 = cs.GetLocalPoint(pnt_03, defaultCameraScale, defaultCameraOffset);
				System.Windows.Point localPnt_04 = cs.GetLocalPoint(pnt_04, defaultCameraScale, defaultCameraOffset);

				dc.PushOpacity(0.3);
				dc.DrawRectangle(null, pen, new Rect(localPnt_01, localPnt_03));
				dc.DrawLine(pen, localPnt_01, localPnt_03);
				dc.DrawLine(pen, localPnt_02, localPnt_04);
				dc.Pop();

				PalletOffset += pallet.Length + DistanceBetweenPallets;
			}
		}

		//=============================================================================
		/// <summary>
		/// Returns point in Warehouse sheet, if rack's sheet is placed at Warehouse sheet
		/// </summary>
		//private static System.Windows.Point GetRackPointRelativeToWarehouseSheet(Rack rack, System.Windows.Point pnt)
		//{
		//	System.Windows.Point warehousePnt = pnt;
		//	if(rack != null && rack.Sheet != null && rack.Sheet.BoundSheetGeometry != null)
		//	{
		//		warehousePnt.X += rack.Sheet.BoundSheetGeometry.TopLeft_GlobalPoint.X;
		//		warehousePnt.Y += rack.Sheet.BoundSheetGeometry.TopLeft_GlobalPoint.Y;
		//	}
		//	
		//	return warehousePnt;
		//}

		//=============================================================================
		/// <summary>
		/// Creates rack advanced properties visual(front and side views)
		/// </summary>
		private static DrawingVisual CreateRackAdvancedPropsVisual(Rack rack, ImageSource watermarkImage, int imageLength, int imageHeight)
		{
			if (rack == null)
				return null;

			//
			DrawingVisual visual = new DrawingVisual();
			using (DrawingContext dc = visual.RenderOpen())
			{
				// draw background
				dc.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(new System.Windows.Point(0.0, 0.0), new System.Windows.Point(imageLength, imageHeight)));

				double exportFontSize = ExportFontSizeDependencies.GetFontSize(imageLength, imageHeight);
				RackAdvancedDrawingSettings drawingSettings = new RackAdvancedDrawingSettings(
					exportFontSize,
					1.2 * exportFontSize,
					2 * exportFontSize,
					exportFontSize,
					1.5 * exportFontSize,
					exportFontSize / 3,
					exportFontSize / 6,
					true,
					false
					);

				// draw rack advanced properties
				RackAdvancedPropertiesControl.sDraw(dc, rack, new System.Windows.Size(imageLength, imageHeight), drawingSettings);

				// draw watermark
				if (watermarkImage != null)
					WatermarkVisual.sDrawWatermark(dc, imageLength, imageHeight, watermarkImage);
			}

			return visual;
		}

		public class RackComparer : IComparer<Rack>
		{
			public int Compare(Rack x, Rack y)
			{
				if (x == null)
					return -1;
				if (y == null)
					return 1;

				// compare index
				if (x.SizeIndex != y.SizeIndex)
					return x.SizeIndex - y.SizeIndex;

				// index is equal
				// compare position in the row
				if (x.IsFirstInRowColumn)
					return -1;
				if (y.IsFirstInRowColumn)
					return 1;

				return 0;
			}
		}

		/// <summary>
		/// Order rectangles to avoid rack and aisle space overlapping
		/// </summary>
		public class RectanglesComparer : IComparer<BaseRectangleGeometry>
		{
			public int Compare(BaseRectangleGeometry x, BaseRectangleGeometry y)
			{
				// Display SheetElevationGeometry over all other geometry, include non initialized geometry.
				if (x is SheetElevationGeometry)
					return 1;
				else if (y is SheetElevationGeometry)
					return -1;

				// Display not initialized geometry over initialized.
				if (x.IsInit != y.IsInit)
				{
					if (x.IsInit)
						return -1;
					return 1;
				}

				// From top to bottom: tie beam, rack, shutter, wall, column, aisle space, all other geometry
				if (x != null && y != null)
				{
					int index_X = GetGeometryOrderIndex(x);
					int index_Y = GetGeometryOrderIndex(y);
					return index_X - index_Y;
				}
				else if (x != null)
					return 1;
				else if (y != null)
					return -1;

				return 0;
			}

			private int GetGeometryOrderIndex(BaseRectangleGeometry geom)
			{
				if (geom == null)
					return 0;

				//if (geom is SheetElevationGeometry)
				//	return 8;
				if (geom is TieBeam)
					return 7;
				else if (geom is Rack)
					return 6;
				else if (geom is Shutter)
					return 5;
				else if (geom is Wall)
					return 4;
				else if (geom is Column)
					return 3;
				else if (geom is AisleSpace)
					return 2;
				else if (geom is Block)
					return 1;

				return 1;
			}
		}
	}

	/// <summary>
	/// Export all sheets layout and each Rack index configuration in the PDF file
	/// </summary>
	public class Command_ExportPDF : Command
	{
		public Command_ExportPDF()
			: base(PackIconKind.FilePdf, "Export PDF") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			if (vm.DrawingControl == null)
				return false;

			if (Utils.FLE(vm.DrawingControl.ActualWidth, 0.0) || Utils.FLE(vm.DrawingControl.ActualHeight, 0.0))
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			if (curDoc.ContainsTieBeamsErrors)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			if (vm.DrawingControl == null)
				return;

			if (Utils.FLE(vm.DrawingControl.ActualWidth, 0.0) || Utils.FLE(vm.DrawingControl.ActualHeight, 0.0))
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			if (curDoc.ContainsTieBeamsErrors)
				return;

			// ENQ number is neccessary for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}

			// export all sheets
			if (curDoc.Sheets == null)
				return;

			//
			string strFile = FileUtils._SaveFileDialog("PDF document |*.pdf", ".pdf", curDoc.CustomerENQ, curDoc.CustomerENQ);
			if (string.IsNullOrEmpty(strFile))
				return;

			// Application theme color should not be applied to PDF export picture.
			// So, save the current theme, set default light theme, export and set current theme back.
			ColorTheme oldTheme = new DefaultLightTheme();
			if (CurrentTheme.CurrentColorTheme != null)
				oldTheme = CurrentTheme.CurrentColorTheme.Clone() as ColorTheme;
			CurrentTheme.CurrentColorTheme = new DefaultLightTheme();
			// Aisle space should be very light for PDF and pictures export
			CurrentTheme.CurrentColorTheme.GeometryColorsTheme.SetGeometryColor(eColorType.eFill_AisleSpace, Colors.Ivory);

			// For PDF and picture export watermark should be lighter
			double oldWatermarkOpacity = WatermarkInfo.Opacity;
			WatermarkInfo.Opacity = 0.07;

			//bool isAllElevationsTogether = curDoc.IsPrintRackElevations;

			try
			{
				// create pdf doc
				PdfDocument pdfDoc = new PdfDocument();

				// list of rack with unique index
				// need for export
				List<Rack> racksList = new List<Rack>();

				int iSheetNumber = 1;

				// export sheets layout and elevations
				foreach (DrawingSheet sheet in curDoc.Sheets)
				{
					if (sheet == null)
						continue;

					if (sheet.Rectangles == null)
						continue;

					if (sheet.Rectangles.Count == 0)
						continue;

					// calculate the size of picture.
					// it should have the same length\height ratio as a sheet.
					// but sheet can has 20000 as a length value, so need to limit max image length\height with 2000 pixels.
					int imageLength = 0;
					int imageHeight = 0;
					//if (Utils.FLT(sheet.Length, Command_ExportImages.maxImageSize) && Utils.FLT(sheet.Width, Command_ExportImages.maxImageSize))
					//{
					//	imageLength = (int)Math.Ceiling((decimal)sheet.Length);
					//	imageHeight = (int)Math.Ceiling((decimal)sheet.Width);
					//}
					//else
					//{
					double sizeScale = 1.0;
					if (Utils.FGT(sheet.Length, sheet.Width))
						sizeScale = (double)Command_ExportImages.maxImageSize / sheet.Length;
					else
						sizeScale = (double)Command_ExportImages.maxImageSize / sheet.Width;
					//
					imageLength = (int)Math.Ceiling(sizeScale * sheet.Length);
					imageHeight = (int)Math.Ceiling(sizeScale * sheet.Width);
					//}

					// List with blocks which should be exported.
					List<ePDFSheetBlock> blocksForExport = new List<ePDFSheetBlock>()
					{
						ePDFSheetBlock.eRackStat,
						ePDFSheetBlock.ePalleteStat,
						ePDFSheetBlock.eMHEDetails,
						ePDFSheetBlock.eImportantNotesOnFloring,
						ePDFSheetBlock.eNotes
					};
					// Remove empty statistics
					if (sheet.RackStatistics == null || sheet.RackStatistics.Count == 0)
						blocksForExport.Remove(ePDFSheetBlock.eRackStat);
					if (sheet.PalletsStatistics == null || sheet.PalletsStatistics.Count == 0)
						blocksForExport.Remove(ePDFSheetBlock.ePalleteStat);
					// Measure height of rack\pallete statistics, MHE details, notes blocks.
					// It is neccessary for calculate - how many PDF sheets need to create and which block can we place at the PDF sheet.
					//
					// key - block
					// value - height in pixels
					Dictionary<ePDFSheetBlock, double> blockToHeightDict = new Dictionary<ePDFSheetBlock, double>();
					bool isDynamicFillBlockHeightInit = false;
					// Height of block, which contains rack\pallete statistics, MHE details, notes blocks.
					double dynamicFillBlockHeight = 0.0;
					foreach(ePDFSheetBlock block in blocksForExport)
					{
						ExportLayoutTemplateVM measureTemplateVM = new ExportLayoutTemplateVM(curDoc, sheet);
						measureTemplateVM.DisplayRackStatistics = false;
						measureTemplateVM.DisplayPalleteStatistics = false;
						measureTemplateVM.DisplayMHEDetails = false;
						measureTemplateVM.DisplayImportantNotesOnFlooring = false;
						measureTemplateVM.DisplayNotes = false;

						if (ePDFSheetBlock.eRackStat == block)
							measureTemplateVM.DisplayRackStatistics = true;
						else if (ePDFSheetBlock.ePalleteStat == block)
							measureTemplateVM.DisplayPalleteStatistics = true;
						else if (ePDFSheetBlock.eMHEDetails == block)
							measureTemplateVM.DisplayMHEDetails = true;
						else if (ePDFSheetBlock.eImportantNotesOnFloring == block)
							measureTemplateVM.DisplayImportantNotesOnFlooring = true;
						else if (ePDFSheetBlock.eNotes == block)
							measureTemplateVM.DisplayNotes = true;

						ExportLayoutTemplate02_Sheet01 measurePDFTemplateControl = new ExportLayoutTemplate02_Sheet01(measureTemplateVM, null);
						_PrepareTemplateForExport(measurePDFTemplateControl);

						if (ePDFSheetBlock.eRackStat == block)
							blockToHeightDict[block] = measurePDFTemplateControl.RackStatBlockHeight;
						else if (ePDFSheetBlock.ePalleteStat == block)
							blockToHeightDict[block] = measurePDFTemplateControl.PalleteStatBlockHeight;
						else if (ePDFSheetBlock.eMHEDetails == block)
							blockToHeightDict[block] = measurePDFTemplateControl.MHEDetailsBlockHeight;
						else if (ePDFSheetBlock.eImportantNotesOnFloring == block)
							blockToHeightDict[block] = measurePDFTemplateControl.ImportantNotesOnFlooringBlockHeight;
						else if (ePDFSheetBlock.eNotes == block)
							blockToHeightDict[block] = measurePDFTemplateControl.NotesBlockHeight;

						if(!isDynamicFillBlockHeightInit)
						{
							isDynamicFillBlockHeightInit = true;
							dynamicFillBlockHeight = measurePDFTemplateControl.DynamicFillBlockHeight;
						}
					}

					string strRackAccessories = GetRackAccessories(curDoc, sheet);
					string strLevelAccessories = GetRackLevelAccessories(sheet);

					// export 1 big picture(sheet layout with vert and horiz sheet elevations
					// or 3 different pictures
					// vert and horiz sheet elevations should be a single page
					List<DrawingVisual> sheetVisualsList = new List<DrawingVisual>();

					if (Utils.FLE(sheet.Length, curDoc.PrintingSheetElevationMaxLength) && Utils.FLE(sheet.Width, curDoc.PrintingSheetElevationMaxHeight))
                        sheetVisualsList.Add(Command_ExportImages.CreateSheetLayoutVisual(sheet, null, imageLength, imageHeight, true, curDoc.IsPrintRackElevations));
                    else
                    {
                        double minUnitsPerPixel = 0.0;

                        int totalImageLength = imageLength - 2 * Command_ExportImages.m_ExtraSpaceForDimension;
                        int totalImageHeight = imageHeight - 2 * Command_ExportImages.m_ExtraSpaceForDimension;

                        double unitPerPixel_SheetLayout = ImageCoordinateSystem.GetMaxUnitsPerPixel(totalImageLength, totalImageHeight, sheet.Length, sheet.Width);

                        double unitPerPixel_HorizSheetElevation = 0.0;
                        double unitPerPixel_VertSheetElevation = 0.0;
                        //
                        List<Rack> horizontalSheetElevationRacksList = null;
                        List<Rack> verticalSheetElevationRacksList = null;
                        double horizontalSheetElevation_BiggestRackHeight = 0.0;
                        double verticalSheetElevation_BiggestRackHeight = 0.0;
                        // Sheet layout and sheet elevations pictures should have the same scale.
                        if (sheet.GetSheetElevations(out horizontalSheetElevationRacksList, out verticalSheetElevationRacksList, out horizontalSheetElevation_BiggestRackHeight, out verticalSheetElevation_BiggestRackHeight))
                        {
                            if (horizontalSheetElevationRacksList != null && horizontalSheetElevationRacksList.Count > 0 && Utils.FGT(horizontalSheetElevation_BiggestRackHeight, 0.0))
                                unitPerPixel_HorizSheetElevation = ImageCoordinateSystem.GetMaxUnitsPerPixel(totalImageLength, totalImageHeight, sheet.Length, horizontalSheetElevation_BiggestRackHeight);

                            if (verticalSheetElevationRacksList != null && verticalSheetElevationRacksList.Count > 0 && Utils.FGT(verticalSheetElevation_BiggestRackHeight, 0.0))
                                unitPerPixel_VertSheetElevation = ImageCoordinateSystem.GetMaxUnitsPerPixel(totalImageLength, totalImageHeight, verticalSheetElevation_BiggestRackHeight, sheet.Width);
                        }

                        minUnitsPerPixel = Math.Max(unitPerPixel_SheetLayout, Math.Max(unitPerPixel_HorizSheetElevation, unitPerPixel_VertSheetElevation));

                        sheetVisualsList.Add(Command_ExportImages.CreateSheetLayoutVisual(sheet, null, imageLength, imageHeight, true, false, minUnitsPerPixel));

                        if (curDoc.IsPrintRackElevations)
                        {
                            if (Utils.FGT(unitPerPixel_HorizSheetElevation, 0.0))
                                sheetVisualsList.Add(Command_ExportImages.CreateSheetElevationVisual(sheet, null, imageLength, imageHeight, true,
                                    horizontalSheetElevationRacksList, horizontalSheetElevation_BiggestRackHeight, minUnitsPerPixel, false));

                            if (Utils.FGT(unitPerPixel_VertSheetElevation, 0.0))
                                sheetVisualsList.Add(Command_ExportImages.CreateSheetElevationVisual(sheet, null, imageLength, imageHeight, false,
                                    verticalSheetElevationRacksList, verticalSheetElevation_BiggestRackHeight, minUnitsPerPixel, false));
                        }
                    }

                    // Create PDF sheets until there are not exported blocks or not exported visuals.
                    // Index of visual for export in sheetVisualsList.
                    int iVisualIndex = 0;
					while (blocksForExport.Count > 0 || iVisualIndex < sheetVisualsList.Count)
					{
						DrawingVisual sheetVisual = null;
						if (iVisualIndex < sheetVisualsList.Count)
						{
							sheetVisual = sheetVisualsList[iVisualIndex];
							++iVisualIndex;
						}
						else
						{
							// If all visuals are already exported then take the first picture, it contains sheet layout.
							sheetVisual = sheetVisualsList[0];
						}

						if (sheetVisual == null)
							break;

						// create page for image
						PdfPage newPage = pdfDoc.AddPage();
						// set view model properties
						ExportLayoutTemplateVM templateVM = new ExportLayoutTemplateVM(curDoc, sheet);
						templateVM.Date = DateTime.Now.Date.ToString("dd MMMM yyyy");
						templateVM.DisplayRackAccessories = true;
						templateVM.DisplayRackLevelAccessories = true;
						templateVM.RackLevelAccessories = strLevelAccessories;
						templateVM.RackAccessories = strRackAccessories;
						templateVM.PageNumber = iSheetNumber;
						templateVM.ImageHeaderText = sheet.Name + ": General Arrangement";

						templateVM.ImageSrc = Command_ExportImages._GetBmpFromVisual(sheetVisual, imageLength, imageHeight);
						templateVM.ParseVisualAccessories();
						
						// Calculate which blocks can be exported
						double availableHeight = dynamicFillBlockHeight;
						bool isAnyBlockExported = false;
						while (blocksForExport.Count > 0)
						{
							// Take the first block, if it cannot be exported, then stop add blocks to this sheet.
							// But at least one block should be exported.
							ePDFSheetBlock block = blocksForExport[0];
							double blockHeight = blockToHeightDict[block];

							if (!isAnyBlockExported || Utils.FLE(blockHeight, availableHeight))
							{
								if (!isAnyBlockExported)
									isAnyBlockExported = true;

								if (ePDFSheetBlock.eRackStat == block)
									templateVM.DisplayRackStatistics = true;
								else if (ePDFSheetBlock.ePalleteStat == block)
									templateVM.DisplayPalleteStatistics = true;
								else if (ePDFSheetBlock.eMHEDetails == block)
									templateVM.DisplayMHEDetails = true;
								else if (ePDFSheetBlock.eImportantNotesOnFloring == block)
									templateVM.DisplayImportantNotesOnFlooring = true;
								else if (ePDFSheetBlock.eNotes == block)
									templateVM.DisplayNotes = true;

								availableHeight -= blockHeight;
								blocksForExport.Remove(block);
							}
							else
								break;
						}

						ExportLayoutTemplate02_Sheet01 pdfExportTemplateControl = new ExportLayoutTemplate02_Sheet01(templateVM, vm.DrawingControl.WatermarkImage);
                        if (sheetVisualsList.IndexOf(sheetVisual) == 0 && templateVM.AccessoriesVisualization.Count > 0)
                        {
							pdfExportTemplateControl.DrawingPart.Content = new ExportTemplate_GeneralDrawingAndAccessories();
						}

						_PrepareTemplateForExport(pdfExportTemplateControl);
						_ExportDrawingVisual(newPage, pdfExportTemplateControl, (int)Math.Floor(pdfExportTemplateControl.DesiredSize.Width), (int)Math.Floor(pdfExportTemplateControl.DesiredSize.Height));

						// Remove image from memory. Otherwise RenderTargetBitmap.Render() can throw OutOfMemory exception.
						templateVM.ImageSrc = null;

						GC.Collect();
						GC.WaitForPendingFinalizers();

						++iSheetNumber;
					}

					// check racks for export
					foreach (BaseRectangleGeometry geom in sheet.Rectangles)
					{
						if (geom == null)
							continue;

						Rack rackGeom = geom as Rack;
						if (rackGeom == null)
							continue;

						Rack foundRack = racksList.Find(r => r.SizeIndex == rackGeom.SizeIndex && r.IsFirstInRowColumn == rackGeom.IsFirstInRowColumn);
						if (foundRack == null)
							racksList.Add(rackGeom);
					}
				}

				// sort racks list
				Command_ExportImages.RackComparer rc = new Command_ExportImages.RackComparer();
				racksList.Sort(rc);

				// export rack advanced properties
				RackAdvancedDrawingSettings drawingSettings = new RackAdvancedDrawingSettings(
					1.5 * 40.0,
					1.5 * 48.0,
					1.5 * 60.0,
					1.5 * 40.0,
					1.5 * 60.0,
					1.5 * 10.0,
					1.5 * 5.0,
					true,
					false
					);

				if (curDoc.IsPrintAllRackElevationsInSinglePage)
				{
					double scaledImageSizeDetailed = Command_ExportImages.maxImageSize / 2.5;
					int scaledImageSize = (int)Math.Ceiling(scaledImageSizeDetailed);

					drawingSettings = new RackAdvancedDrawingSettings(
						(1.5 * 40.0) / 2.5,
						(1.5 * 48.0) / 2.5,
						(1.5 * 60.0) / 2.5,
						(1.5 * 40.0) / 2.5,
						(1.5 * 60.0) / 2.5,
						(1.5 * 10.0) / 2.5,
						(1.5 * 5.0) / 2.5,
						true,
						false
						);

					for (int i = 0; i < racksList.Count; i++)
                    {
						Rack secondRack = null;
						DrawingVisual secondRackVisual = null;

						Rack rack = racksList[i];

						if (rack == null)
							continue;

						// create 2000x2000 image
						DrawingVisual rackVisual = _CreateRackAdvancedPropsVisual(rack, null, scaledImageSize, scaledImageSize, drawingSettings);
						if (rackVisual == null)
							continue;

						i++;
                        if (i < racksList.Count)
                        {
							secondRack = racksList[i];

							if (secondRack != null)
								secondRackVisual = _CreateRackAdvancedPropsVisual(secondRack, null, scaledImageSize, scaledImageSize, drawingSettings);
						}

						string firstRackAccessories = GetRackLevelAccessories(rack);

						string secondRackAccessories = secondRackVisual != null 
							? GetRackLevelAccessories(secondRack) 
							: string.Empty;

						PdfPage newPage = pdfDoc.AddPage();

						ExportLayoutTemplateVM templateVM = new ExportLayoutTemplateVM(curDoc, null);
						templateVM.Date = DateTime.Now.Date.ToString("dd MMMM yyyy");
						templateVM.PageNumber = iSheetNumber;
						templateVM.DisplayRackStatistics = false;
						templateVM.DisplayPalleteStatistics = false;
						templateVM.DisplayMHEDetails = false;
						templateVM.DisplayImportantNotesOnFlooring = false;
						templateVM.DisplayNotes = false;
						templateVM.DisplayRackAccessories = false;
						templateVM.DisplayRackLevelAccessories = true;
						
						templateVM.ImageHeaderText = secondRackVisual != null 
							? $"{rack.Text} - {secondRack.Text}" 
							: rack.Text;

						templateVM.RackLevelAccessories = $"{secondRackAccessories}\n\n{secondRackAccessories}";

						if (secondRackVisual != null)
						{
							templateVM.ImageSources = new List<ImageSource>();

							templateVM.ImageSources.Add(
								Command_ExportImages._GetBmpFromVisual(rackVisual, scaledImageSize, scaledImageSize));

							templateVM.ImageSources.Add(
								Command_ExportImages._GetBmpFromVisual(secondRackVisual, scaledImageSize, scaledImageSize));
                        }
                        else
                        {
							templateVM.ImageSrc = Command_ExportImages._GetBmpFromVisual(rackVisual, Command_ExportImages.maxImageSize, Command_ExportImages.maxImageSize);
						}

						ExportLayoutTemplate02_Sheet01 pdfExportTemplateControl = new ExportLayoutTemplate02_Sheet01(templateVM, vm.DrawingControl.WatermarkImage);

						if (secondRackVisual != null)
						{
							pdfExportTemplateControl.DrawingPart.Content = new ExportTemplate_MultipleRacksLayout();
						}

						_PrepareTemplateForExport(pdfExportTemplateControl);
						//
						// rack.Text contains text that displayed over rack in the sheet layout
						_ExportDrawingVisual(newPage, pdfExportTemplateControl, (int)Math.Floor(pdfExportTemplateControl.DesiredSize.Width), (int)Math.Floor(pdfExportTemplateControl.DesiredSize.Height));

						// Remove image from memory. Otherwise RenderTargetBitmap.Render() can throw OutOfMemory exception.
						templateVM.ImageSrc = null;
						GC.Collect();
						GC.WaitForPendingFinalizers();

						++iSheetNumber;
					}
				}
				else
				{
					foreach (Rack rack in racksList)
					{
						if (rack == null)
							continue;

						// create 2000x2000 image
						DrawingVisual rackVisual = _CreateRackAdvancedPropsVisual(rack, null, Command_ExportImages.maxImageSize, Command_ExportImages.maxImageSize, drawingSettings);
						if (rackVisual == null)
							continue;

						// create page for image
						PdfPage newPage = pdfDoc.AddPage();

						// set view model properties
						ExportLayoutTemplateVM templateVM = new ExportLayoutTemplateVM(curDoc, null);
						templateVM.Date = DateTime.Now.Date.ToString("dd MMMM yyyy");
						templateVM.PageNumber = iSheetNumber;
						templateVM.ImageHeaderText = rack.Text;
						templateVM.DisplayRackStatistics = false;
						templateVM.DisplayPalleteStatistics = false;
						templateVM.DisplayMHEDetails = false;
						templateVM.DisplayImportantNotesOnFlooring = false;
						templateVM.DisplayNotes = false;
						templateVM.DisplayRackAccessories = false;
						templateVM.DisplayRackLevelAccessories = true;
						templateVM.RackLevelAccessories = GetRackLevelAccessories(rack);
						templateVM.ImageSrc = Command_ExportImages._GetBmpFromVisual(rackVisual, Command_ExportImages.maxImageSize, Command_ExportImages.maxImageSize);
						//
						System.Windows.Controls.UserControl pdfExportTemplateControl = new ExportLayoutTemplate02_Sheet01(templateVM, vm.DrawingControl.WatermarkImage);

						_PrepareTemplateForExport(pdfExportTemplateControl);
						//
						// rack.Text contains text that displayed over rack in the sheet layout
						_ExportDrawingVisual(newPage, pdfExportTemplateControl, (int)Math.Floor(pdfExportTemplateControl.DesiredSize.Width), (int)Math.Floor(pdfExportTemplateControl.DesiredSize.Height));

						// Remove image from memory. Otherwise RenderTargetBitmap.Render() can throw OutOfMemory exception.
						templateVM.ImageSrc = null;
						GC.Collect();
						GC.WaitForPendingFinalizers();

						++iSheetNumber;
					}
				}

				// pdfDoc.Save() trows an exception if PDF doesnt have sheets
				if (iSheetNumber == 0)
					return;

				pdfDoc.Save(strFile);
			}
			catch { }
			finally
			{
				if (oldTheme != null)
					CurrentTheme.CurrentColorTheme = oldTheme;

				// restore old watermark opacity
				WatermarkInfo.Opacity = oldWatermarkOpacity;
			}
		}

		/// <summary>
		/// Enum for blocks, which can be placed in the right column of PDF sheet.
		/// </summary>
		private enum ePDFSheetBlock : int
		{
			eRackStat = 1,
			ePalleteStat = 2,
			eMHEDetails = 3,
			eImportantNotesOnFloring = 4,
			eNotes = 5
		}

		//=============================================================================
		/// <summary>
		/// Export DrawingVisual in the separate pdf page.
		/// </summary>
		private static void _ExportDrawingVisual(PdfPage pdfPage, Visual visualForExport, int imageLength, int imageHeight)
		{
			if (visualForExport == null)
				return;

			if (imageLength == 0 || imageHeight == 0)
				return;

			if (pdfPage == null)
				return;

			// Make it before call bitmap.Render(), because otherwise bitmap.Render() can throw OutOfMemory exception.
			GC.Collect();
			GC.WaitForPendingFinalizers();

			//
			pdfPage.Width = imageLength;
			pdfPage.Height = imageHeight;

			// dont pass sheet.Length and sheet.Width as image size, because they can has 20000 units value
			var bitmap = new RenderTargetBitmap(imageLength, imageHeight, 96, 96, PixelFormats.Default);
			bitmap.Render(visualForExport);
			//
			var encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmap));

			using (Stream stream = new MemoryStream())
			{
				encoder.Save(stream);

				// Get an XGraphics object for drawing
				using (XGraphics gfx = XGraphics.FromPdfPage(pdfPage))
				{
					// draw image
					using (XImage image = XImage.FromStream(stream))
					{
						gfx.DrawImage(image, 0, 0, imageLength, imageHeight);
					}
				}
			}

			bitmap = null;
		}

		//=============================================================================
		private static DrawingVisual _CreateRackAdvancedPropsVisual(Rack rack, ImageSource watermarkImage, int imageLength, int imageHeight, RackAdvancedDrawingSettings drawingSettings)
		{
			if (rack == null)
				return null;

			//
			DrawingVisual visual = new DrawingVisual();
			using (DrawingContext dc = visual.RenderOpen())
			{
				// draw background
				dc.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(new System.Windows.Point(0.0, 0.0), new System.Windows.Point(imageLength, imageHeight)));

				// draw rack advanced properties
				RackAdvancedPropertiesControl.sDraw(dc, rack, new System.Windows.Size(imageLength, imageHeight), drawingSettings);

				// draw watermark
				if (watermarkImage != null)
					WatermarkVisual.sDrawWatermark(dc, imageLength, imageHeight, watermarkImage);
			}

			return visual;
		}

		//=============================================================================
		private static void _PrepareTemplateForExport(System.Windows.Controls.UserControl exportTemplateControl)
		{
			if (exportTemplateControl == null)
				return;

			exportTemplateControl.Background = new SolidColorBrush(Colors.White);
			exportTemplateControl.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
			exportTemplateControl.Arrange(new Rect(exportTemplateControl.DesiredSize));
			exportTemplateControl.InvalidateVisual();
			exportTemplateControl.UpdateLayout();
		}

		//=============================================================================
		/// <summary>
		/// String with rack accessories, they are common for all racks at the sheet.
		/// It is used in sheet PDF export.
		/// </summary>
		private static string GetRackAccessories(DrawingDocument doc, DrawingSheet sheet)
		{
			StringBuilder sb = new StringBuilder();

			if(doc != null && doc.Rack_Accessories != null)
			{
				if (doc.Rack_Accessories.UprightGuard)
					sb.Append("Upright Guard\n");
				if (doc.Rack_Accessories.RowGuard)
				{
					if (doc.Rack_Accessories.IsHeavyDutyEnabled)
						sb.Append("Row Guard Heavy Duty\n");
					else
						sb.Append("Row Guard\n");
				}
				if (doc.Rack_Accessories.Signages)
					sb.Append("Signages\n");
				if (doc.Rack_Accessories.IsMeshCladdingEnabled)
					sb.Append("Mesh Cladding\n");
			}

			if(sheet != null && sheet.TieBeamsList != null && sheet.TieBeamsList.Count > 0)
				sb.Append("Tie Beam\n");

			return sb.ToString();
		}

		//=============================================================================
		/// <summary>
		/// String with rack level accessory.
		/// </summary>
		private static string GetRackLevelAccessories(Rack rack)
		{
			if (rack == null || rack.Levels == null)
				return string.Empty;

			// key - accessory name
			// value - count
			Dictionary<string, int> accessoryToCountDict = new Dictionary<string, int>();
			accessoryToCountDict.Add(RackLevelAccessories.DECKING_PANEL_6BP_SHELVING, 0);
			accessoryToCountDict.Add(RackLevelAccessories.DECKING_PANEL_6BP_PALLET, 0);
			accessoryToCountDict.Add(RackLevelAccessories.DECKING_PANEL_4BP, 0);
			accessoryToCountDict.Add(RackLevelAccessories.PALLET_STOPPER, 0);
			accessoryToCountDict.Add(RackLevelAccessories.FORK_ENTRY_BAR, 0);
			accessoryToCountDict.Add(RackLevelAccessories.PALLET_SUPPORT_BAR, 0);
			accessoryToCountDict.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER, 0);
			accessoryToCountDict.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB, 0);
			accessoryToCountDict.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB, 0);

			foreach(RackLevel level in rack.Levels)
			{
				if (level == null)
					continue;

				if (level.Accessories == null)
					continue;

				if(level.Accessories.IsDeckPlateAvailable)
				{
					if (eDeckPlateType.eAlongDepth_UDL == level.Accessories.DeckPlateType)
						++accessoryToCountDict[RackLevelAccessories.DECKING_PANEL_6BP_SHELVING];
					else if(eDeckPlateType.eAlongDepth_PalletSupport == level.Accessories.DeckPlateType)
						++accessoryToCountDict[RackLevelAccessories.DECKING_PANEL_6BP_PALLET];
					else if(eDeckPlateType.eAlongLength == level.Accessories.DeckPlateType)
						++accessoryToCountDict[RackLevelAccessories.DECKING_PANEL_4BP];
				}
				if(level.Accessories.PalletStopper)
					++accessoryToCountDict[RackLevelAccessories.PALLET_STOPPER];
				if(level.Accessories.ForkEntryBar)
					++accessoryToCountDict[RackLevelAccessories.FORK_ENTRY_BAR];
				if(level.Accessories.PalletSupportBar)
					++accessoryToCountDict[RackLevelAccessories.PALLET_SUPPORT_BAR];
				if(level.Accessories.GuidedTypePalletSupport)
				{
					if(level.Accessories.GuidedTypePalletSupport_WithPSB && level.Accessories.GuidedTypePalletSupport_WithStopper)
						++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB];
					else if(level.Accessories.GuidedTypePalletSupport_WithPSB)
						++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB];
					else if(level.Accessories.GuidedTypePalletSupport_WithStopper)
						++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER];
					else
						++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB];
				}

				if (rack.AreLevelsTheSame)
					break;
			}

			// build accessory string
			StringBuilder sb = new StringBuilder();

			foreach (string strKey in accessoryToCountDict.Keys)
			{
				if (string.IsNullOrEmpty(strKey))
					continue;

				if (accessoryToCountDict[strKey] > 0)
				{
					sb.Append(strKey);
					sb.Append("\n");
				}
			}

			return sb.ToString();
		}
		/// <summary>
		/// Check all racks in the sheet and returns all enabled level accessories
		/// </summary>
		private static string GetRackLevelAccessories(DrawingSheet sheet)
		{
			if (sheet == null)
				return string.Empty;

			List<Rack> racksList = sheet.GetAllRacks();
			if (racksList == null)
				return string.Empty;

			// key - accessory name
			// value - count
			Dictionary<string, int> accessoryToCountDict = new Dictionary<string, int>();
			accessoryToCountDict.Add(RackLevelAccessories.DECKING_PANEL_6BP_SHELVING, 0);
			accessoryToCountDict.Add(RackLevelAccessories.DECKING_PANEL_6BP_PALLET, 0);
			accessoryToCountDict.Add(RackLevelAccessories.DECKING_PANEL_4BP, 0);
			accessoryToCountDict.Add(RackLevelAccessories.PALLET_STOPPER, 0);
			accessoryToCountDict.Add(RackLevelAccessories.FORK_ENTRY_BAR, 0);
			accessoryToCountDict.Add(RackLevelAccessories.PALLET_SUPPORT_BAR, 0);
			accessoryToCountDict.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER, 0);
			accessoryToCountDict.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB, 0);
			accessoryToCountDict.Add(RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB, 0);

			foreach (Rack rack in racksList)
			{
				if (rack == null)
					continue;
				if (rack.Levels == null)
					continue;

				foreach (RackLevel level in rack.Levels)
				{
					if (level == null)
						continue;

					if (level.Accessories == null)
						continue;

					if (level.Accessories.IsDeckPlateAvailable)
					{
						if (eDeckPlateType.eAlongDepth_UDL == level.Accessories.DeckPlateType)
							++accessoryToCountDict[RackLevelAccessories.DECKING_PANEL_6BP_SHELVING];
						else if (eDeckPlateType.eAlongDepth_PalletSupport == level.Accessories.DeckPlateType)
							++accessoryToCountDict[RackLevelAccessories.DECKING_PANEL_6BP_PALLET];
						else if (eDeckPlateType.eAlongLength == level.Accessories.DeckPlateType)
							++accessoryToCountDict[RackLevelAccessories.DECKING_PANEL_4BP];
					}
					if (level.Accessories.PalletStopper)
						++accessoryToCountDict[RackLevelAccessories.PALLET_STOPPER];
					if (level.Accessories.ForkEntryBar)
						++accessoryToCountDict[RackLevelAccessories.FORK_ENTRY_BAR];
					if (level.Accessories.PalletSupportBar)
						++accessoryToCountDict[RackLevelAccessories.PALLET_SUPPORT_BAR];
					if (level.Accessories.GuidedTypePalletSupport)
					{
						if (level.Accessories.GuidedTypePalletSupport_WithPSB && level.Accessories.GuidedTypePalletSupport_WithStopper)
							++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER_AND_PSB];
						else if (level.Accessories.GuidedTypePalletSupport_WithPSB)
							++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB];
						else if (level.Accessories.GuidedTypePalletSupport_WithStopper)
							++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_STOPPER];
						else
							++accessoryToCountDict[RackLevelAccessories.GUIDED_TYPE_PALLET_SUPPORT_WITH_PSB];
					}

					if (rack.AreLevelsTheSame)
						break;
				}
			}

			// build accessory string
			StringBuilder sb = new StringBuilder();

			foreach (string strKey in accessoryToCountDict.Keys)
			{
				if (string.IsNullOrEmpty(strKey))
					continue;

				if (accessoryToCountDict[strKey] > 0)
				{
					sb.Append(strKey);
					sb.Append("\n");
				}
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Copy selected rectangles in current sheet
	/// </summary>
	public class Command_CopySelection : Command
	{
		public Command_CopySelection()
			: base(PackIconKind.ContentCopy, "Copy") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return false;

			if (curSheet.SelectedGeometryCollection.Count == 0)
				return false;

			// Disable copy-paste columns.
			// If selection contains only columns and nothing else then doesnt execute cop-paste.
			BaseRectangleGeometry notColumnGeom = curSheet.SelectedGeometryCollection.FirstOrDefault(g => g != null && !(g is Column));
			if (notColumnGeom == null)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			if (curSheet.SelectedGeometryCollection.Count == 0)
				return;

			curDoc.Set_ShowAdvancedProperties(false, false);

			// Disable copy-paste columns.
			// If selection contains only columns and nothing else then doesnt execute cop-paste.
			BaseRectangleGeometry notColumnGeom = curSheet.SelectedGeometryCollection.FirstOrDefault(g => g != null && !(g is Column));
			if (notColumnGeom == null)
				return;

			curSheet.CopySelectedGeometry();
		}
	}

	/// <summary>
	/// Paste copied rectangles in current sheet
	/// </summary>
	public class Command_PasteCopiedGeometry : Command
	{
		public Command_PasteCopiedGeometry()
			: base(PackIconKind.ContentPaste, "Paste") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			if (curDoc.CopiedGeomList.Count == 0)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			curDoc.Set_ShowAdvancedProperties(false, false);
			curSheet.PasteGeometry();
		}
	}

	/// <summary>
	/// Delete selected rectangles in current sheet
	/// </summary>
	public class Command_DeleteSelection : Command
	{
		public Command_DeleteSelection()
			: base(PackIconKind.Delete, "Delete") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return false;

			if (curSheet.SelectedGeometryCollection.Count == 0)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			if (curSheet.SelectedGeometryCollection.Count == 0)
				return;

			curDoc.Set_ShowAdvancedProperties(false, false);

			curSheet.DeleteGeometry(curSheet.SelectedGeometryCollection.ToList(), true, true);
		}
	}

	public class Command_RackMatchProperties : Command
	{
		public Command_RackMatchProperties()
			: base(PackIconKind.SwapHorizontal, "Rack match properties") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return false;

			// onlu initialized geometry in selection
			if (curSheet.NonInitSelectedGeometryList.Count > 0)
				return false;

			// Selection should be empty or contain only one rack
			if (curSheet.SelectedGeometryCollection.Count == 0)
				return true;
			if (curSheet.SelectedGeometryCollection.Count == 1 && curSheet.SelectedGeometryCollection[0] is Rack)
				return true;

			return false;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.GeometryMatchProperties();
		}
	}

	public class Command_MoveSelectedGeometry : Command
	{
		public Command_MoveSelectedGeometry()
			: base(PackIconKind.CursorMove, "Move selected geometry") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return false;

			if (curSheet.SelectedGeometryCollection.Count > 0)
				return true;

			return false;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.MoveSelection();
		}
	}

	/// <summary>
	/// Set 1:1 camera scale
	/// </summary>
	public class Command_ZoomAll : Command
	{
		public Command_ZoomAll()
			: base(PackIconKind.AspectRatio, "Zoom all") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			if (curDoc.ShowAdvancedProperties)
				return false;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			if (curSheet.IsSheetFullyDisplayed)
				return;

			curSheet.FullyDisplaySheet();
			curSheet.MarkStateChanged();
		}
	}

	/// <summary>
	/// Set camera scale and offset by selecting rectangle
	/// </summary>
	public class Command_ZoomWindow : Command
	{
		public Command_ZoomWindow()
			: base(PackIconKind.CropSquare, "Zoom window") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return false;

			if (curDoc.ShowAdvancedProperties)
				return false;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);

			if (DrawingDocument._sDrawing != null)
				DrawingDocument._sDrawing.ZoomWindow();
		}
	}

	/// <summary>
	/// Display dialog with document settings - back to back distance, pallet size, etc.
	/// </summary>
	public class Command_DocumentSettings : Command
	{
		public Command_DocumentSettings()
			: base(PackIconKind.Gear, "Document settings") { }

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null)
				return;

			// Double call DialogHost.Show() throw exceptions
			// For example - rename sheet and close application.
			if (vm.DlgHost != null && vm.DlgHost.IsOpen)
				return;

			//
			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);
			//
			ePalletType oldPalletType = curDoc.RacksPalletType;
			double oldOverhangValue = curDoc.RacksPalletsOverhangValue;
			// remove selection
			if (curDoc.CurrentSheet != null)
				curDoc.CurrentSheet.SelectedGeometryCollection.Clear();

			DocumentSettingsViewModel docSettingsVM = new DocumentSettingsViewModel(curDoc);
			DocumentSettingsDialog docSettingsDialog = new DocumentSettingsDialog(docSettingsVM);
			
			//show the dialog
			// true - save
			// false - cancel
			// null - continue
			var result = await DialogHost.Show(docSettingsDialog);
			if (result is bool && (bool)result)
			{
				// set new values
				curDoc.RacksPalletType = docSettingsVM.RacksPalletsType;
				curDoc.RacksPalletsOverhangValue = docSettingsVM.RacksPalletsOverhangValue;
				//
				curDoc.Currency = docSettingsVM.Currency;
				curDoc.Rate = docSettingsVM.Rate;
				curDoc.Discount = docSettingsVM.Discount;
				// no need update MHEConfigurationsColl, because it was edit by reference

				// update pdf printing settings
				curDoc.IsPrintRackElevations = docSettingsVM.IsPrintRackElevations;
				curDoc.IsPrintAllRackElevationsInSinglePage = docSettingsVM.IsPrintAllRackElevationsInSinglePage;
				curDoc.PrintingSheetElevationMaxLength = docSettingsVM.PrintingSheetElevationMaxLength;
				curDoc.PrintingSheetElevationMaxHeight = docSettingsVM.PrintingSheetElevationMaxHeight;

				//
				foreach (PalletConfiguration palletConfig in docSettingsVM.PalletConfigurationCollection)
				{
					if (palletConfig == null)
						continue;

					if (palletConfig.MarkDeleted)
					{
						curDoc.PalletConfigurationCollection.Remove(palletConfig);
						continue;
					}

					PalletConfiguration foundPC = curDoc.PalletConfigurationCollection.FirstOrDefault(p => p != null && p.GUID == palletConfig.GUID);
					if(foundPC == null)
						curDoc.PalletConfigurationCollection.Add(palletConfig);
				}

				// recalc pallets depth only if pallet type or overhang was changed
				bool bRecalcPalletWidth = false;
				if (oldPalletType != curDoc.RacksPalletType || (curDoc.RacksPalletType == ePalletType.eOverhang && Utils.FNE(oldOverhangValue, curDoc.RacksPalletsOverhangValue)))
					bRecalcPalletWidth = true;

				// check layout
				await curDoc.CheckDocument(false, bRecalcPalletWidth);
			}

			// Mark state changed
			curDoc.MarkStateChanged();
		}
	}

	//=============================================================================
	// Save current pallet configuration to the document PalletConfigurationCollection.
	public class Command_SavePalletConfiguration : Command
	{
		public Command_SavePalletConfiguration()
			: base(PackIconKind.Add, "Save pallet configuration") { }

		protected override void _Execute(object parameter)
		{
			Pallet pallet = parameter as Pallet;
			if (pallet == null)
				return;

			if (pallet.Level == null
				|| pallet.Level.Owner == null
				|| pallet.Level.Owner.Sheet == null
				|| pallet.Level.Owner.Sheet.Document == null
				|| pallet.Level.Owner.Sheet.Document.PalletConfigurationCollection == null)
				return;

			if (pallet.Level.Owner.Sheet.Document.ContainsPalletConfig(pallet.Length, pallet.Width, pallet.Height, pallet.Load))
				return;

			int index = Utils.GetUniquePalletConfigurationIndex(pallet.Level.Owner.Sheet.Document.PalletConfigurationCollection);
			if (index < 1)
				return;

			PalletConfiguration newPC = new PalletConfiguration(index);
			newPC.Length = pallet.Length;
			newPC.Width = pallet.Width;
			newPC.Height = pallet.Height;
			newPC.Capacity = pallet.Load;
			//
			pallet.Level.Owner.Sheet.Document.PalletConfigurationCollection.Add(newPC);
			// document mark state changed will be called inside
			pallet.PalletConfiguration = newPC;
		}
	}

	//=============================================================================
	// Unbind current pallet from selected pallet configuration.
	public class Command_UnbindPalletConfiguration : Command
	{
		public Command_UnbindPalletConfiguration()
			: base(PackIconKind.Minus, "Unbind pallet configuration") { }

		protected override void _Execute(object parameter)
		{
			Pallet pallet = parameter as Pallet;
			if (pallet == null)
				return;

			// document mark state changed will be called inside
			pallet.PalletConfiguration = null;
		}
	}

	public class Command_DeletePalletConfiguration : Command
	{
		public Command_DeletePalletConfiguration()
			: base(PackIconKind.Delete, "Delete") { }

		protected override void _Execute(object parameter)
		{
			PalletConfiguration pc = parameter as PalletConfiguration;
			if (pc == null)
				return;

			pc.MarkDeleted = true;
		}
	}

	public class Command_RestorePalletConfiguration : Command
	{
		public Command_RestorePalletConfiguration()
			: base(PackIconKind.Restore, "Restore") { }

		protected override void _Execute(object parameter)
		{
			PalletConfiguration pc = parameter as PalletConfiguration;
			if (pc == null)
				return;

			pc.MarkDeleted = false;
		}
	}

	/// <summary>
	/// Display edit sheet's roof dialog
	/// </summary>
	public class Command_SheetRoof : Command
	{
		public Command_SheetRoof()
			: base(PackIconKind.HomeVariantOutline, "Edit sheet roof") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return false;

			return true;
		}

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return;

			// Double call DialogHost.Show() throw exceptions
			// For example - rename sheet and close application.
			if (vm.DlgHost != null && vm.DlgHost.IsOpen)
				return;

			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);

			EditRoofDialogViewModel roofDialogVM = new EditRoofDialogViewModel(curDoc.CurrentSheet);
			EditRoofDialog roofDlg = new EditRoofDialog(roofDialogVM);
			roofDlg.InvalidateArrange();
			roofDlg.InvalidateMeasure();
			roofDlg.UpdateLayout();

			//
			var result = await DialogHost.Show(roofDlg);

			// check all racks height
			List<Rack> racksList = curDoc.CurrentSheet.GetAllRacks();
			if (racksList != null)
			{
				foreach (Rack rack in racksList)
				{
					if (rack == null)
						continue;

					rack.CheckRackHeight();
				}
			}
			curDoc.CurrentSheet.CheckTieBeams();

			// Mark state changed
			curDoc.CurrentSheet.MarkStateChanged();
		}
	}

	/// <summary>
	/// Display edit sheet's notes dialog
	/// </summary>
	public class Command_SheetNotes : Command
	{
		public Command_SheetNotes()
			: base(PackIconKind.NoteText, "Edit notes") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return false;

			return true;
		}

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return;

			// Double call DialogHost.Show() throw exceptions
			// For example - rename sheet and close application.
			if (vm.DlgHost != null && vm.DlgHost.IsOpen)
				return;

			curDoc.IsInCommand = true;
			curDoc.Set_ShowAdvancedProperties(false, false);

			EditSheetNotesDailogViewModel notesDialogVM = new EditSheetNotesDailogViewModel(curDoc.CurrentSheet);
			EditSheetNotesDialog notesDialog = new EditSheetNotesDialog(notesDialogVM);
			//roofDlg.InvalidateArrange();
			//roofDlg.InvalidateMeasure();
			//roofDlg.UpdateLayout();

			//
			var result = await DialogHost.Show(notesDialog);
			if (result is bool && (bool)result)
			{
				curDoc.CurrentSheet.Notes = notesDialogVM.Notes;
				// Mark state changed
				curDoc.CurrentSheet.MarkStateChanged();
			}
			else
				curDoc.SetTheLastState();
		}
	}

	/// <summary>
	/// Read Columns data from LoadChart.xlsx file. It takes around 1 minute.
	/// Available only in the DEBUG-mode.
	/// </summary>
	public class Command_ReadDataFromExcel : Command
	{
		public Command_ReadDataFromExcel()
			: base(PackIconKind.FileExcelBox, "Read data from the LoadChart.xlsx") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return false;

			// Document should contain only 2 sheets: warehouse and empty sheet.
			if (curDoc.Sheets.Count > 2)
				return false;

			foreach (DrawingSheet sheet in curDoc.Sheets)
			{
				if (sheet == null)
					continue;

				if (sheet.Rectangles.Count != 0)
					return false;
			}

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return;

			curDoc.ReadColumnsFromExcel();
		}
	}

	/// <summary>
	/// Export data to the excel file using PRDBOM_App.exe
	/// </summary>
	public class Command_CreateBOM : Command
	{
		public Command_CreateBOM()
			: base(PackIconKind.FileExcel, "Create BOM") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return false;

			if (curDoc.ContainsTieBeamsErrors)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return;

			if (curDoc.ContainsTieBeamsErrors)
				return;

			// ENQ number is neccessaru for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}
			// Check that ENQ number can be used as a filename for the text file.
			if (curDoc.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				vm.DisplayMessageDialog("ENQ number will be used as a filename, but it has incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
				return;
			}

			if (!vm.CreateBOM())
				vm.DisplayMessageDialog("An error ocurred while create BOM.");
		}
	}

	/// <summary>
	/// Export data to the dwg file using PRDApp.exe
	/// </summary>
	public class Command_CreateDWG : Command
	{
		public Command_CreateDWG()
			: base(PackIconKind.AlphabetABox, "Create dwg file") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return false;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return false;

			if (curDoc.ContainsTieBeamsErrors)
				return false;

			return true;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			DrawingDocument curDoc = vm.CurrentDocument;
			if (curDoc == null || curDoc.CurrentSheet == null)
				return;

			if (curDoc.ContainsTieBeamsErrors)
				return;

			// ENQ number is neccessaru for any kind of export.
			// Display the message if it is empty.
			if (string.IsNullOrEmpty(curDoc.CustomerENQ))
			{
				vm.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
				return;
			}
			// Check that ENQ number can be used as a filename for the text file.
			if (curDoc.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				vm.DisplayMessageDialog("ENQ number will be used as a filename, but it has incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
				return;
			}

			if (!vm.CreateDWG())
				vm.DisplayMessageDialog("An error ocurred while create dwg file.");
		}
	}

	/// <summary>
	/// Displays application colors edit dialog.
	/// </summary>
	public class Command_ApplicationTheme : Command
	{
		public Command_ApplicationTheme()
			: base(PackIconKind.ColorLens, "Edit application theme") { }

		//=============================================================================
		protected override async void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm == null)
				return;

			// Double call DialogHost.Show() throw exceptions
			// For example - rename sheet and close application.
			if (vm.DlgHost != null && vm.DlgHost.IsOpen)
				return;

			ColorTheme theme = null;
			if(CurrentTheme.CurrentColorTheme != null)
				theme = CurrentTheme.CurrentColorTheme.Clone() as ColorTheme;
			if (theme == null)
				theme = new DefaultLightTheme();

			EditAppThemeViewModel dialogVM = new EditAppThemeViewModel(theme);
			EditAppThemeDialog dialog = new EditAppThemeDialog(dialogVM);
			//roofDlg.InvalidateArrange();
			//roofDlg.InvalidateMeasure();
			//roofDlg.UpdateLayout();

			//
			var result = await DialogHost.Show(dialog);
			if (result is bool && (bool)result)
			{
				// apply new theme
				CurrentTheme.CurrentColorTheme = dialogVM.ColorTheme;
			}
		}
	}

	/// <summary>
	/// Save application colors theme to the file
	/// </summary>
	public class Command_SaveTheme : Command
	{
		public Command_SaveTheme()
			: base(PackIconKind.ContentSave, "Save theme")
		{ }

		protected override void _Execute(object parameter)
		{
			EditAppThemeViewModel vm = parameter as EditAppThemeViewModel;
			if (vm == null)
				return;
			if (vm.ColorTheme == null)
				return;

			string strFilePath = FileUtils._SaveFileDialog("Text file |*.txt", ".txt", null, "ApplicationTheme.txt");
			if (string.IsNullOrEmpty(strFilePath))
				return;

			vm.ColorTheme.WriteToFile(strFilePath);
		}
	}

	/// <summary>
	/// Open application colors theme file
	/// </summary>
	public class Command_OpenTheme : Command
	{
		public Command_OpenTheme()
			: base(PackIconKind.FolderOpen, "Open theme")
		{ }

		protected override void _Execute(object parameter)
		{
			EditAppThemeViewModel vm = parameter as EditAppThemeViewModel;
			if (vm == null)
				return;

			string strFilePath = FileUtils._OpenFileDialog("Text file |*.txt", ".txt", "ApplicationTheme.txt");
			if (string.IsNullOrEmpty(strFilePath))
				return;

			ColorTheme newTheme = new DefaultLightTheme();
			if (!newTheme.ReadFromFile(strFilePath))
				return;

			vm.ColorTheme = newTheme;
		}
	}
}