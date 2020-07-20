using DrawingControl;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RackDrawingApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindow_ViewModel m_VM = null;

		public MainWindow(MainWindow_ViewModel vm)
		{
			InitializeComponent();
			SourceInitialized += OnSourceInitialized;

			m_VM = vm;
			m_VM.DlgHost = DlgHost;
			m_VM.DrawingControl = this.Drawing;
			this.DataContext = m_VM;

			//
			this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
			this.SizeChanged += MainWindow_SizeChanged;
			this.KeyDown += MainWindow_KeyDown;
			this.Loaded += MainWindow_Loaded;
			this.ContentRendered += MainWindow_ContentRendered;
		}

		//=============================================================================
		private IntPtr m_WndHandle = IntPtr.Zero;

		//=============================================================================
		void OnSourceInitialized(object sender, EventArgs e)
		{
			System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(this).Handle);
			source.AddHook(new System.Windows.Interop.HwndSourceHook(WndProc));

			//
			m_WndHandle = new WindowInteropHelper(this).Handle;
			WindowsUtils.DisableMaximizeButton(m_WndHandle);
			WindowsUtils.EnableMinimizeButton(m_WndHandle);
		}

		//=============================================================================
		const int WM_NCLBUTTONDBLCLK = 0x00A3;
		const int WM_SYSCOMMAND = 0x0112;
		const int SC_MOVE = 0xF010;
		const int SC_RESTORE = 0xF120;
		//=============================================================================
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_SYSCOMMAND:
					int command = wParam.ToInt32() & 0xfff0;
					if (command == SC_MOVE)
					{
						// prevent user from moving the window
						handled = true;
					}
					else if (command == SC_RESTORE && WindowState == WindowState.Maximized)
					{
						// prevent user from restoring the window while it is maximized
						// (but allow restoring when it is minimized)
						handled = true;
					}
					break;
				case WM_NCLBUTTONDBLCLK:
					handled = true;  //prevent double click from minimazing the window.
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}


		//=============================================================================
		private DrawingSheet _Get_CurrentSheet()
		{
			DrawingSheet _currSheet = null;

			//
			if (m_VM != null && m_VM.CurrentDocument != null)
				_currSheet = m_VM.CurrentDocument.CurrentSheet;

			return _currSheet;
		}

		//=============================================================================
		private void MainWindow_ContentRendered(object sender, EventArgs e)
		{
			// Try to open drawing document which was passed through command line arguments.
			if (m_VM != null && !string.IsNullOrEmpty(UserInfo.DrawingPath))
			{
				try
				{
					System.Windows.Application.Current.Dispatcher.Invoke(new Action(async () =>
					{
						await m_VM.OpenDrawing(UserInfo.DrawingPath);
					}
					)
					);
				}
				catch { }
			}
		}

		//=============================================================================
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			// MainWindow_ViewModel was created before MainWindow.
			// MainWindow_ViewModel read DrawingDocument and tried to display DrawingDocument.CurrentSheet when 
			// DrawingDocument._sDrawing is null. So CurrentSheet will be displayed without any content.
			// Hide and show CurrentSheet when MainWindow is loaded.
			if (m_VM != null && m_VM.CurrentDocument != null && m_VM.CurrentDocument.CurrentSheet != null)
			{
				m_VM.CurrentDocument.CurrentSheet.Show(false);
				m_VM.CurrentDocument.CurrentSheet.Show(true);
			}
		}

		//=============================================================================
		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			//
			DrawingSheet currSheet = _Get_CurrentSheet();
			if (currSheet == null)
				return;

			DrawingDocument currDoc = currSheet.Document;
			if (currDoc == null)
				return;

			// Ignore all command shortcuts if AdvancedProperties tab is displayed.
			// Otherwise user can press "Delete" button while AdvancedProperties tab is displayed - 
			// selected rack will be deleted - and user will see empty AdvancedProperties tab without return button.
			if (currDoc.ShowAdvancedProperties)
				return;

			if(!e.Handled)
			{
				if(e.Key == Key.Delete)
				{
					currSheet.DeleteGeometry(currSheet.SelectedGeometryCollection.ToList(), true, true);
					e.Handled = true;
				}
				else if(e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
				{
					currSheet.CopySelectedGeometry();
					e.Handled = true;
				}
				else if(e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
				{
					if (currDoc.CopiedGeomList.Count > 0)
					{
						currSheet.PasteGeometry();
						e.Handled = true;
					}
				}

				Drawing.SendKey(e);
			}
		}

		//=============================================================================
		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Drawing.UpdateDrawing(false);
		}

		//=============================================================================
		private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// if no one handle it then send it to DrawingControl
			if (!e.Handled && e.OriginalSource != Drawing)
			{
				Drawing.RaiseEvent(e);
			}
		}

		//=============================================================================
		private void BlockButton_Click(object sender, RoutedEventArgs e)
		{
			//
			DrawingSheet _currSheet = _Get_CurrentSheet();
			if (_currSheet == null)
				return;

			_currSheet.CreateBlock();
		}

		//=============================================================================
		private void ColumnButton_Click(object sender, RoutedEventArgs e)
		{
			//
			DrawingSheet _currSheet = _Get_CurrentSheet();
			if (_currSheet == null)
				return;

			_currSheet.CreateColumn();
		}

		//=============================================================================
		private async void CloseSheettButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null || m_VM.CurrentDocument == null)
				return;

			if (m_VM.CurrentDocument.Sheets.Count == 1)
				return;

			Button btn = sender as Button;
			if (btn == null)
				return;

			ListBoxItem lbi = Utils.TryFindParent<ListBoxItem>(btn);
			if (lbi == null)
				return;
			
			DrawingSheet sheetToClose = lbi.DataContext as DrawingSheet;
			if (sheetToClose == null)
				return;
			
			if (sheetToClose.Rectangles.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Sheet \"");
				sb.Append(sheetToClose.DisplayName);
				sb.Append("\" has geometry inside. Do you want to close it? All geometry inside will be deleted with the sheet.");
			
				SaveChangesDialog_ViewModel vm = new SaveChangesDialog_ViewModel();
				vm.Text = sb.ToString();
				vm.IsSaveButtonVisible = false;
			
				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(vm);
			
				//show the dialog
				// true - save
				// false - cancel
				// null - continue
				var result = await DialogHost.Show(saveChangesDialog);
			
				if (result is bool)
				{
					bool bRes = (bool)result;
			
					// cancel - dont close document
					if (!bRes)
						return;
				}
			}

			// select document
			m_VM.CurrentDocument.RemoveSheet(sheetToClose);
			m_VM.CurrentDocument.MarkStateChanged();
		}

		//=============================================================================
		// Window closing event cant await for save changes dialog.
		// So mark event as canceled and call save changes dialog.
		// If user click "cancel" then it is nothing to do - event salready canceled.
		// If user click "continue" then set m_ShouldClose to "true" and try to close application again.
		// At the second try m_ShouldClose != null and we will not enter in save changes dialog section.
		//
		// m_ShouldClose is marker
		// - null - mark event canceled and call save changes dialog
		// - not null - do nothing and let window close
		private bool? m_ShouldClose = null;
		private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (m_ShouldClose == null)
			{
				// Double call DialogHost.Show() throw exceptions
				// For example - rename sheet and close application.
				if (m_VM != null && m_VM.DlgHost != null && m_VM.DlgHost.IsOpen)
				{
					e.Cancel = true;
					return;
				}

				// mark comment to m_ShouldClose
				e.Cancel = true;

				// is here unsaved documents?
				bool bUnsaved = false;
				if (m_VM != null && m_VM.CurrentDocument != null)
				{
					if (m_VM.CurrentDocument.HasChanges)
						bUnsaved = true;
				}

				//
				if (bUnsaved)
				{
					SaveChangesDialog_ViewModel vm = new SaveChangesDialog_ViewModel();
					vm.Text = "This document is unsaved and contains some changes. Some data will be lost possibly.";
					vm.IsSaveButtonVisible = false;

					SaveChangesDialog saveChangesDialog = new SaveChangesDialog(vm);

					//show the dialog
					// true - save
					// false - cancel
					// null - continue
					var result = await DialogHost.Show(saveChangesDialog);

					if (result is bool && !(bool)result)
						return;
				}

				m_ShouldClose = true;
				Application.Current.Shutdown();
			}
		}

		//=============================================================================
		private void RackButton_Click(object sender, RoutedEventArgs e)
		{
			//
			DrawingSheet _currSheet = _Get_CurrentSheet();
			if (_currSheet == null)
				return;

			_currSheet.CreateRack();
		}

		//=============================================================================
		private void AisleSpaceButton_Click(object sender, RoutedEventArgs e)
		{
			//
			DrawingSheet _currSheet = _Get_CurrentSheet();
			if (_currSheet == null)
				return;

			_currSheet.CreateAisleSpace();
		}

		//=============================================================================
		private void ShutterButton_Click(object sender, RoutedEventArgs e)
		{
			//
			DrawingSheet _currSheet = _Get_CurrentSheet();
			if (_currSheet == null)
				return;

			_currSheet.CreateShutter();
		}

		//=============================================================================
		private void WallButton_Click(object sender, RoutedEventArgs e)
		{
			//
			DrawingSheet _currSheet = _Get_CurrentSheet();
			if (_currSheet == null)
				return;

			_currSheet.CreateWall();
		}

		//=============================================================================
		private async void PlaceSheetButton_Click(object sender, RoutedEventArgs e)
		{
			//
			WarehouseSheet warehouseSheet = _Get_CurrentSheet() as WarehouseSheet;
			if (warehouseSheet == null)
				return;
			if (warehouseSheet.Document == null)
				return;

			PlaceSheetDialogVM dialogVM = new PlaceSheetDialogVM(warehouseSheet.Document);
			PlaceSheetDialog psDialog = new PlaceSheetDialog(dialogVM);

			// true - OK
			// false - CANCEL
			var result = await DialogHost.Show(psDialog);
			if (result is bool && (bool)result)
			{
				if (dialogVM.SelectedSheetPreview == null)
					return;

				warehouseSheet.CreateSheetGeometry(dialogVM.SelectedSheetPreview.Sheet);
			}
		}

		//=============================================================================
		private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			TextBox tb = sender as TextBox;
			if (tb != null)
				tb.SelectAll();
		}

		//=============================================================================
		private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				if (!tb.IsKeyboardFocusWithin)
				{
					e.Handled = true;
					tb.Focus();
				}
			}
		}

		//=============================================================================
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox tb = sender as TextBox;
			if (tb == null)
				return;

			if (e.Key == Key.Enter)
			{
				BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
				if (be != null)
					be.UpdateSource();

				tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));

				//
				e.Handled = true;
				return;
			}

			if(e.Key == Key.Escape)
			{
				BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
				if (be != null)
					be.UpdateTarget();

				//
				e.Handled = true;
				return;
			}
		}

		//=============================================================================
		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			TextBox tb = sender as TextBox;
			if (tb == null)
				return;

			Property_ViewModel pvm = tb.DataContext as Property_ViewModel;
			if (pvm == null)
				return;

			if (!pvm.IsNumeric)
				return;

			bool bAccept = false;
			// is digit
			if ((e.Key >= Key.D0 && e.Key <= Key.D9)
				|| (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
			{
				bAccept = true;
			}
			//
			if (e.Key == Key.Escape || e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Back)
				bAccept = true;

			if (!bAccept)
				e.Handled = true;
		}

		//=============================================================================
		private async void SheetTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				DrawingSheet _sheet = null;
				TextBlock tb = sender as TextBlock;
				if(tb != null)
					_sheet = tb.DataContext as DrawingSheet;

				if (_sheet != null && _sheet.Document != null)
				{
					SheetNameDialog_ViewModel vm = new SheetNameDialog_ViewModel(_sheet);
					vm.Name = _sheet.Name;

					SheetName_Dialog _SheetNameDialog = new SheetName_Dialog(vm);

					// true - OK
					// false - CANCEL
					var result = await DialogHost.Show(_SheetNameDialog);
					if (result is bool && (bool)result)
					{
						_sheet.Name = vm.Name;
						_sheet.MarkStateChanged();
					}
				}

				e.Handled = true;
			}
		}

		//=============================================================================
		private void AdvancedPropertiesButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM != null && m_VM.CurrentDocument != null)
			{
				m_VM.CurrentDocument.ShowAdvancedProperties = true;
				if (DrawingDocument._sDrawing != null)
				{
					DrawingDocument._sDrawing.InvalidateVisual();
					DrawingDocument._sDrawing.UpdateDrawing(true);
				}
			}
		}

		//=============================================================================
		private void BackToSimplePropertiesButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM != null && m_VM.CurrentDocument != null)
			{
				m_VM.CurrentDocument.ShowAdvancedProperties = false;
				if (DrawingDocument._sDrawing != null)
				{
					DrawingDocument._sDrawing.InvalidateVisual();
					DrawingDocument._sDrawing.UpdateDrawing(true);
				}
			}
		}

		//=============================================================================
		private async void LevelAccessoriesButton_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null)
				return;

			MainWindow_ViewModel mainVM = btn.DataContext as MainWindow_ViewModel;
			if (mainVM == null)
				return;

			DrawingDocument curDoc = mainVM.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			if (curSheet.SelectedGeometryCollection.Count != 1)
				return;

			Rack selectedRack = curSheet.SelectedGeometryCollection[0] as Rack;
			if (selectedRack == null)
				return;

			RackLevel selectedLevel = selectedRack.SelectedLevel;
			if (selectedLevel == null)
				return;

			// clone accessories value
			RackLevelAccessories accValue = selectedLevel.Accessories.Clone() as RackLevelAccessories;
			if (accValue == null)
				return;
			accValue.Owner = selectedLevel;
			RackLevelAccessoriesDialog_ViewModel vm = new RackLevelAccessoriesDialog_ViewModel(accValue, selectedRack.ShowPallet);
			RackLevelAccessoriesDialog levelAccDialog = new RackLevelAccessoriesDialog(vm);

			//show the dialog
			// true - OK
			// false - CANCEL
			var result = await DialogHost.Show(levelAccDialog);
			if (result is bool && (bool)result)
			{
				if(selectedLevel.Accessories != vm.Accessories)
					selectedLevel.Accessories = vm.Accessories;
			}
		}

		//=============================================================================
		private async void RackAccessoriesButton_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null)
				return;

			MainWindow_ViewModel mainVM = btn.DataContext as MainWindow_ViewModel;
			if (mainVM == null)
				return;

			DrawingDocument curDoc = mainVM.CurrentDocument;
			if (curDoc == null)
				return;

			DrawingSheet curSheet = curDoc.CurrentSheet;
			if (curSheet == null)
				return;

			if (curSheet.SelectedGeometryCollection.Count != 1)
				return;

			Rack selectedRack = curSheet.SelectedGeometryCollection[0] as Rack;
			if (selectedRack == null)
				return;

			RackAccessories rackAcc = selectedRack.Accessories.Clone() as RackAccessories;
			if (rackAcc == null)
				return;
			RackAccessories_ViewModel vm = new RackAccessories_ViewModel(rackAcc);
			RackAccessoriesDialog rackAccDialog = new RackAccessoriesDialog(vm);

			//show the dialog
			// true - OK
			// false - CANCEL
			var result = await DialogHost.Show(rackAccDialog);
			if (result is bool && (bool)result)
			{
				selectedRack.Accessories = vm.Accessories;
			}
		}

		//=============================================================================
		private void GoToBoundSheetButton_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null)
				return;

			MainWindow_ViewModel mainVM = btn.DataContext as MainWindow_ViewModel;
			if (mainVM == null)
				return;

			DrawingDocument curDoc = mainVM.CurrentDocument;
			if (curDoc == null)
				return;

			WarehouseSheet curSheet = curDoc.CurrentSheet as WarehouseSheet;
			if (curSheet == null || curSheet.SingleSelectedGeometry == null)
				return;

			SheetGeometry sheetGeometry = curSheet.SingleSelectedGeometry as SheetGeometry;
			if (sheetGeometry == null)
				return;
			if (sheetGeometry.BoundSheet == null)
				return;

			curDoc.CurrentSheet = sheetGeometry.BoundSheet;
		}

		//=============================================================================
		private void CreateSheetButton_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null)
				return;

			MainWindow_ViewModel mainVM = btn.DataContext as MainWindow_ViewModel;
			if (mainVM == null)
				return;

			DrawingDocument curDoc = mainVM.CurrentDocument;
			if (curDoc == null)
				return;

			WarehouseSheet curSheet = curDoc.CurrentSheet as WarehouseSheet;
			if (curSheet == null || curSheet.SingleSelectedGeometry == null)
				return;

			SheetGeometry sheetGeometry = curSheet.SingleSelectedGeometry as SheetGeometry;
			if (sheetGeometry == null)
				return;
			if (sheetGeometry.BoundSheet != null)
				return;

			DrawingSheet newSheet = new DrawingSheet(curDoc);
			if(newSheet != null)
			{
				newSheet.Set_Length((UInt32)Utils.GetWholeNumber(sheetGeometry.Length_X), false, false);
				newSheet.Set_Width((UInt32)Utils.GetWholeNumber(sheetGeometry.Length_Y), false, false);
				curDoc.AddSheet(newSheet, false);
				sheetGeometry.BoundSheet = newSheet;
				curDoc.MarkStateChanged();
			}
		}

		//=============================================================================
		private void Rack3DViewToggleButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton tb = sender as ToggleButton;
			if (tb == null)
				return;
			if (!(bool)tb.IsChecked)
				return;

			if (m_VM == null)
				return;

			m_VM.Viewport3DContent = RackAppViewport3D.eViewportContent.eSelectedRack;
		}

		//=============================================================================
		private void CloseSheet3DViewButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_VM == null)
				return;

			m_VM.Display3DViewControl = false;

			if (m_VM.CurrentDocument != null)
				m_VM.CurrentDocument.IsInCommand = false;
		}
	}
}
