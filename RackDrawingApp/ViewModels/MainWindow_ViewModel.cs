using DrawingControl;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace RackDrawingApp
{
	public class MainWindow_ViewModel : BaseViewModel, IDisplayDialog
	{
		public MainWindow_ViewModel()
		{
			_InitializeDocumentTemplate();
			
			m_CurrentDocument = CreateNewDocument();

			AppCloseTimer.SetBeforeShutdownDelegate(OnBeforeAppShutdown);
		}

		#region Properties

		//=============================================================================
		/// <summary>
		/// If true then need to display ApplicationVersion at MainWindow.
		/// </summary>
		public bool IsItDEBUG
		{
			get
			{
#if DEBUG
				return true;
#endif

				return false;
			}
		}
		//=============================================================================
		/// <summary>
		/// Application version, which is displayed in DEBUG mode only.
		/// </summary>
		public string ApplicationVersion { get { return "RackDrawingApp 1.407.041"; } }

		//=============================================================================
		public DialogHost DlgHost { get; set; }

		//=============================================================================
		/// <summary>
		/// Default document template. It is used for new document creating.
		/// 
		/// If create document though default constructor "= new DrawingDocument(this)" then
		/// RackLoadUtils.ReadData() will be called inside. It reads data from the excel file and it takes around 1 minute at my PC.
		/// It is too long.
		/// So, lets create DrawingDocument template, read data from excel and save this empty file as a template.
		/// On application start read this template from stream. Template already contains data from the excel file, so we dont spend 1 minute
		/// to read excel file.
		/// </summary>
		private DrawingDocument m_DocumentTemplate = null;

		//=============================================================================
		public DrawingControl.DrawingControl DrawingControl { get; set; }

		//=============================================================================
		private DrawingDocument m_CurrentDocument = null;
		public DrawingDocument CurrentDocument
		{
			get { return m_CurrentDocument; }
			set
			{
				// remove old graphics
				if (m_CurrentDocument != null && m_CurrentDocument.CurrentSheet != null)
					m_CurrentDocument.CurrentSheet.Show(false);

				m_CurrentDocument = value;
				if (m_CurrentDocument != null)
				{
					DrawingSheet currSheet = m_CurrentDocument.CurrentSheet;
					if (currSheet != null)
						currSheet.Show(true);
				}

				NotifyPropertyChanged(() => CurrentDocument);
				NotifyPropertyChanged(() => WindowTitle);
			}
		}

		//=============================================================================
		public string WindowTitle
		{
			get
			{
				string strTitle = "RackDrawingApp";

				if(m_CurrentDocument != null)
				{
					strTitle += " - ";

					strTitle += m_CurrentDocument.DisplayName;
					if (m_CurrentDocument.IsItNewDocument)
						strTitle += "*";
				}

				return strTitle;
			}
		}

		//=============================================================================
		private ObservableCollection<ICommand> m_commands = new ObservableCollection<ICommand>()
		{
			new Command_NewDocument(),
			new Command_Open(),
			new Command_Save(),
			//new Command_SaveAs(),
			new Command_IncreaseRevisionSave(),
			new Command_NewSheet(),
#if DEBUG
			new Command_ReadDataFromExcel(),
			new Command_ApplicationTheme(),
#endif
			null,
			new Command_Undo(),
			new Command_Redo(),
			null,
			new Command_CopySelection(),
			new Command_PasteCopiedGeometry(),
			new Command_DeleteSelection(),
			new Command_RackMatchProperties(),
			new Command_MoveSelectedGeometry(),
			null,
			new Command_ZoomAll(),
			new Command_ZoomWindow(),
			null,
#if DEBUG
			new Command_ExportToTXT(),
#endif
			new Command_CreateBOM(),
			new Command_CreateDWG(),
			new Command_ExportImages(),
			new Command_ExportPDF(),
			null,
			new Command_CustomerInfo(),
			new Command_DocumentSettings(),
			new Command_SheetRoof(),
			new Command_SheetNotes()
		};
		public ObservableCollection<ICommand> Commands { get { return m_commands; } }

		#endregion

		#region Public functions

		//=============================================================================
		public void OnDocumentSaved()
		{
			NotifyPropertyChanged(() => WindowTitle);
		}

		//=============================================================================
		public async Task<bool> OpenDrawing(string strDrawingPath)
		{
			DrawingDocument.ClearErrors();

			if (string.IsNullOrEmpty(strDrawingPath))
				return false;

			if (!File.Exists(strDrawingPath))
			{
				await this.DisplayMessageDialog("Cant open document \"" + strDrawingPath + "\". File doesnt exist.");
				return false;
			}

			//
			bool bIsNewDocOpened = false;
			try
			{
				FileStream fs = new FileStream(strDrawingPath, FileMode.Open);
				if (fs != null)
				{

					BinaryFormatter bf = new BinaryFormatter();
					DrawingDocument doc = (DrawingDocument)bf.Deserialize(fs);
					doc.DisplayDialog = this;
					doc.Path = strDrawingPath;

					// save old document if new document is not correct and user dont want to fix it
					DrawingDocument oldDoc = this.CurrentDocument;
					//
					this.CurrentDocument = doc;
					bool bCorrect = await doc.CheckDocument(true, false);
					// if opened document is not correct then set old document
					if (!bCorrect)
						this.CurrentDocument = oldDoc;
					else
					{
						doc.MarkStateChanged();
						doc.RemoveAllStatesExceptCurrent();
						bIsNewDocOpened = true;

						// Clear changed sheets guids.
						DrawingSheet.m_ChangedSheetsGuidsSet.Clear();
					}

					//
					fs.Close();
					fs.Dispose();
				}
			}
			catch { }

			//
			if (DrawingDocument._sDontSupportDocument)
			{
				// open document which is not supported
				SaveChangesDialog_ViewModel saveChanges_VM = new SaveChangesDialog_ViewModel();
				saveChanges_VM.Text = "This document has very old version, its not supported. Save this document in this application possibly lead to data loss.";
				saveChanges_VM.IsSaveButtonVisible = false;
				saveChanges_VM.IsCancelButtonVisible = false;

				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(saveChanges_VM);

				//show the dialog
				// true - save
				// false - cancel
				// null - continue
				var result = await DialogHost.Show(saveChangesDialog);
			}
			else if (DrawingDocument._sBiggerMajorNumber > 0)
			{
				await this.DisplayMessageDialog("An error occurred while opening document. Document was created in the new version of the application and doesnt supported in this application version.\nDocument was not opened.");
				return false;
			}
			else if (DrawingDocument._sNewVersion_StreamRead > 0)
			{
				// open new document in old version application
				SaveChangesDialog_ViewModel saveChanges_VM = new SaveChangesDialog_ViewModel();
				saveChanges_VM.Text = "This document was created\\saved in the new version of application. It can be drawn or working incorrect. Save this document in this application possibly lead to data loss.";
				saveChanges_VM.IsSaveButtonVisible = false;
				saveChanges_VM.IsCancelButtonVisible = false;

				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(saveChanges_VM);

				//show the dialog
				// true - save
				// false - cancel
				// null - continue
				var result = await DialogHost.Show(saveChangesDialog);
			}
			else if (DrawingDocument._sStreamReadException > 0)
			{
				//
				SaveChangesDialog_ViewModel saveChanges_VM = new SaveChangesDialog_ViewModel();
				saveChanges_VM.Text = "Exceptions ocurred while open document. It can be drawn or working incorrect. Save this document in this application possibly lead to data loss.";
				saveChanges_VM.IsSaveButtonVisible = false;
				saveChanges_VM.IsCancelButtonVisible = false;

				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(saveChanges_VM);

				//show the dialog
				// true - save
				// false - cancel
				// null - continue
				var result = await DialogHost.Show(saveChangesDialog);
			}
			else
			{
				if (bIsNewDocOpened && this.CurrentDocument != null)
				{
					// ENQ number is neccessaru for any kind of export.
					// Display the message if it is empty.
					if (string.IsNullOrEmpty(this.CurrentDocument.CustomerENQ))
						await this.DisplayMessageDialog("ENQ number is neccessary for any kind of export. Please go to the Customer Info and input ENQ number.");
					else
					{
						// Check that ENQ number can be used as a filename for the text file.
						if (this.CurrentDocument.CustomerENQ.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
							await this.DisplayMessageDialog("ENQ number contains incorrect characters for the filename. Please go to the Customer Info and edit ENQ number.");
						else if (this.CurrentDocument.NameWithoutExtension != this.CurrentDocument.CustomerENQ)
							await this.DisplayMessageDialog("ENQ number is different from the document name. Please use ENQ number as a document name.");
					}

					return true;
				}
			}

			return false;
		}

		//=============================================================================
		public Task<object> YesNoCancelDialog(string strContent, out IYesNoCancelViewModel vm)
		{
			vm = null;

			if (DlgHost == null)
				return null;

			try
			{
				YesNoDialog_ViewModel dialogVM = new YesNoDialog_ViewModel(strContent);
				vm = dialogVM;
				YesNo_Dialog dialog = new YesNo_Dialog(dialogVM);

				return DialogHost.Show(dialog);
			}
			catch { }

			return null;
		}

		//=============================================================================
		public Task<object> DisplayMessageDialog(string strMessage)
		{
			try
			{
				DisplayMessageDialog_ViewModel dialogVM = new DisplayMessageDialog_ViewModel(strMessage);
				DisplayMessageDialog dialog = new DisplayMessageDialog(dialogVM);

				Task<object> task = DialogHost.Show(dialog);
				return task;
			}
			catch { }

			return null;
		}

		//=============================================================================
		public DrawingDocument CreateNewDocument()
		{
			DrawingDocument newDocument = null;
			if(m_DocumentTemplate != null)
			{
				//newDocument = Utils.DeepClone<DrawingDocument>(m_DocumentTemplate);
				newDocument = m_DocumentTemplate.Clone() as DrawingDocument;
				newDocument.DisplayDialog = this;

				// Make new DrawingSheet.GUID for all sheets in the template.
				if (newDocument != null && newDocument.Sheets != null)
				{
					foreach (DrawingSheet sheet in newDocument.Sheets)
					{
						if (sheet == null)
							continue;

						sheet.CreateNewGUID();
					}
				}
			}

			return newDocument;
		}

		//=============================================================================
		/// <summary>
		/// Export to the excel file using PRDBOM_App.exe, BOM_Temp.xlsx template abd "Master BOM.xlsx".
		/// </summary>
		public bool CreateBOM()
		{
			DrawingDocument currDoc = this.CurrentDocument;
			if (currDoc == null)
				return false;

			// ENQ number is neccessary for any kind of export.
			// Text document should have name like ENQ number.
			string textFileName = currDoc.CustomerENQ;

			if (string.IsNullOrEmpty(textFileName))
				return false;

			string strAppFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string strDefaultFolder = FileUtils.BuildDefaultDirectory(currDoc.CustomerENQ);

			bool result = false;
			try
			{
				// Check that ENQ number is correct as a filename.
				if (textFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					return false;
				}

				// STEP 1.
				// Copy PRDBOM_App.exe.
				string PRDBOM_App_FilePath = strAppFolder + "\\Resources\\PRDBOM_App.exe";
				if (File.Exists(PRDBOM_App_FilePath))
				{
					string NewPRDBOM_App_FilePath = strDefaultFolder + "\\PRDBOM_App.exe";
					File.Copy(PRDBOM_App_FilePath, NewPRDBOM_App_FilePath, true);

					if (File.Exists(NewPRDBOM_App_FilePath))
					{
						// STEP 2.
						// Copy BOM_Temp.slsx.
						try
						{
							string BOM_Temp_FilePath = strAppFolder + "\\Resources\\BOM_Temp.xlsx";
							if (File.Exists(BOM_Temp_FilePath))
							{
								string NewBOM_Temp_FilePath = strDefaultFolder + "\\BOM_Temp.xlsx";
								File.Copy(BOM_Temp_FilePath, NewBOM_Temp_FilePath, true);

								if (File.Exists(NewBOM_Temp_FilePath))
								{
									// STEP 3.
									// Copy "Master BOM.xlsx".
									try
									{
										string masterBOMFilePath = strAppFolder + "\\Resources\\Master BOM.xlsx";
										if (File.Exists(masterBOMFilePath))
										{
											string strNewMasterBOMFilePath = strDefaultFolder + "\\Master BOM.xlsx";
											File.Copy(masterBOMFilePath, strNewMasterBOMFilePath, true);

											if (File.Exists(strNewMasterBOMFilePath))
											{
												// STEP 4.
												// Export txt file in the same folder.
												string textFilePath = strDefaultFolder + "\\" + textFileName + ".txt";
												if (currDoc.ExportToTxt(textFilePath))
												{
													// STEP 5.
													// Run exe file.
													try
													{
														using (Process exeProcess = Process.Start(NewPRDBOM_App_FilePath))
														{
															exeProcess.WaitForExit();
															result = true;
														}
													}
													catch { }
												}

												try
												{
													File.Delete(textFilePath);
												}
												catch { }
											}

											// Delete copied "Master BOM.xlsx"
											try
											{
												File.Delete(strNewMasterBOMFilePath);
											}
											catch { }
										}
									}
									catch { }

									// Delete copied "BOM_Temp.xlsx" 
									try
									{
										File.Delete(NewBOM_Temp_FilePath);
									}
									catch { }
								}
							}
						}
						catch { }

						// Delete copied "PRDBOM_App.exe"
						try
						{
							File.Delete(NewPRDBOM_App_FilePath);
						}
						catch { }
					}
				}
			}
			catch { }

			return result;
		}
		//=============================================================================
		/// <summary>
		/// Export to the dwg file using PRDApp.exe, PRD_Temp.dwt template and LayoutDrawing.dll.
		/// </summary>
		public bool CreateDWG()
		{
			DrawingDocument currDoc = this.CurrentDocument;
			if (currDoc == null)
				return false;

			// ENQ number is neccessary for any kind of export.
			// Text document should have name like ENQ number.
			string textFileName = currDoc.CustomerENQ;

			if (string.IsNullOrEmpty(textFileName))
				return false;

			string strAppFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string strDefaultFolder = FileUtils.BuildDefaultDirectory(currDoc.CustomerENQ);

			bool result = false;
			try
			{
				// Check that ENQ number is correct as a filename.
				if (textFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					return false;
				}

				// STEP 1.
				// Copy PRDApp.exe.
				string PRDApp_FilePath = strAppFolder + "\\Resources\\PRDApp.exe";
				if (File.Exists(PRDApp_FilePath))
				{
					string NewPRDApp_FilePath = strDefaultFolder + "\\PRDApp.exe";
					File.Copy(PRDApp_FilePath, NewPRDApp_FilePath, true);

					if (File.Exists(NewPRDApp_FilePath))
					{
						// STEP 2.
						// Copy LayoutDrawing.dll
						try
						{
							string LayoutDrawing_FilePath = strAppFolder + "\\Resources\\LayoutDrawing.dll";
							if (File.Exists(LayoutDrawing_FilePath))
							{
								string NewLayoutDrawing_FilePath = strDefaultFolder + "\\LayoutDrawing.dll";
								File.Copy(LayoutDrawing_FilePath, NewLayoutDrawing_FilePath, true);

								if (File.Exists(NewLayoutDrawing_FilePath))
								{
									// STEP 3.
									// Copy PRD_Temp.dwt
									try
									{
										string PRD_Temp_FilePath = strAppFolder + "\\Resources\\PRD_Temp.dwt";
										if (File.Exists(PRD_Temp_FilePath))
										{
											string NewPRD_Temp_FilePath = strDefaultFolder + "\\PRD_Temp.dwt";
											File.Copy(PRD_Temp_FilePath, NewPRD_Temp_FilePath, true);

											if (File.Exists(NewPRD_Temp_FilePath))
											{
												// STEP 3.
												// Export txt file in the same folder.
												string textFilePath = strDefaultFolder + "\\" + textFileName + ".txt";
												if (currDoc.ExportToTxt(textFilePath))
												{
													// STEP 4.
													// Run exe file.
													try
													{
														using (Process exeProcess = Process.Start(NewPRDApp_FilePath))
														{
															exeProcess.WaitForExit();
															result = true;
														}
													}
													catch { }
												}

												try
												{
													File.Delete(textFilePath);
												}
												catch { }

												// Delete copied "PRD_Temp.dwt"
												try
												{
													File.Delete(NewPRD_Temp_FilePath);
												}
												catch { }
											}
										}
									}
									catch { }

									// Delete copied "LayoutDrawing.dll"
									try
									{
										File.Delete(NewLayoutDrawing_FilePath);
									}
									catch { }
								}
							}
						}
						catch { }

						// Delete copied PRDApp.exe""
						try
						{
							File.Delete(NewPRDApp_FilePath);
						}
						catch { }
					}
				}
			}
			catch { }

			return result;
		}

		#endregion

		#region Private and protected functions

		//=============================================================================
		/// <summary>
		/// Initialize document template which is used for creating new documents.
		/// </summary>
		private void _InitializeDocumentTemplate()
		{
			// STEP 1.
			// Try to read template from the embedded resource.
			Assembly assembly = Assembly.GetExecutingAssembly();
			string resourceName = "RackDrawingApp.Properties.DocumentTemplate.rda";

			// Debug code - displays all resources in the assembly.
			//List<string> strResourcesList = new List<string>();
			//foreach (string s in assembly.GetManifestResourceNames())
			//	strResourcesList.Add(s);

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				try
				{
					DrawingDocument.ClearErrors();

					BinaryFormatter bf = new BinaryFormatter();
					m_DocumentTemplate = (DrawingDocument)bf.Deserialize(stream);


					// Use this template if no errors occurred while read template.
					if(DrawingDocument._sDontSupportDocument
						|| DrawingDocument._sNewVersion_StreamRead > 0
						|| DrawingDocument._sStreamReadException > 0)
					{
						m_DocumentTemplate = null;
					}
				}
				catch { }
			}

			// STEP 2.
			// There was problem when read document template from the embedded resource.
			// Create document template using default constructor and read excel file.
			if(m_DocumentTemplate == null)
				m_DocumentTemplate = new DrawingDocument(this);
			
			// Set parameters.
			m_DocumentTemplate.CustomerName = UserInfo.CustomerName;
			m_DocumentTemplate.CustomerENQ = UserInfo.EnqNo;
			m_DocumentTemplate.CustomerContactNo = UserInfo.CustomerContactNo;
			m_DocumentTemplate.CustomerEMail = UserInfo.CustomerEmailID;
			m_DocumentTemplate.CustomerAddress = UserInfo.CustomerBillingAddress;
			m_DocumentTemplate.CustomerSite = UserInfo.CustomerSiteAddress;
		}

		//=============================================================================
		private Task<object> OnBeforeAppShutdown()
		{
			if (m_CurrentDocument == null)
				return null;

			// Ask for save before app close.
			if (m_CurrentDocument.HasChanges)
			{
				// undo all not completed changes
				m_CurrentDocument.SetTheLastState();

				SaveChangesDialog_ViewModel saveChangesDialogVM = new SaveChangesDialog_ViewModel();
				saveChangesDialogVM.Text = "Application session is limited by 24 hours. Application will be closed. Do you want to save changes?";
				saveChangesDialogVM.IsSaveButtonVisible = true;
				saveChangesDialogVM.IsCancelButtonVisible = false;

				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(saveChangesDialogVM);

				// Close previous dialog.
				if (DlgHost != null && DlgHost.IsOpen)
					DlgHost.IsOpen = false;

				return DialogHost.Show(saveChangesDialog).ContinueWith<object>((previousTask) =>
				{
					var prevTaskResult = previousTask.Result;
					// true - save
					// false - cancel
					// null - continue
					if (prevTaskResult is bool && (bool)prevTaskResult)
					{
						// save the document
						string strFilePath = string.Empty;
						if (m_CurrentDocument.IsItNewDocument || m_CurrentDocument.NameWithoutExtension != m_CurrentDocument.CustomerENQ)
						{
							string strOldPath = null;
							if (!m_CurrentDocument.IsItNewDocument && !string.IsNullOrEmpty(m_CurrentDocument.Path))
								strOldPath = m_CurrentDocument.Path;

							strFilePath = FileUtils._SaveFileDialog(DrawingDocument.FILE_FILTER, DrawingDocument.FILE_EXTENSION, m_CurrentDocument.CustomerENQ, m_CurrentDocument.CustomerENQ, strOldPath);
							if (string.IsNullOrEmpty(strFilePath))
								return null;
						}

						// Try to save in UI thread, otherwise DrawingDocument.Save throws an exception on DrawingControl.Update() method which updates UI.
						if (DrawingControl != null)
						{
							DrawingControl.Dispatcher.Invoke(new Action(() =>
							{
								m_CurrentDocument.Save(strFilePath);
							}
							));

							return true;
						}
						else
							return m_CurrentDocument.Save(strFilePath);
					}

					return null;
				});
			}
			else
				return this.DisplayMessageDialog("Application session is limited by 24 hours. Application will be closed.");

			return null;
		}

		#endregion
	}
}
