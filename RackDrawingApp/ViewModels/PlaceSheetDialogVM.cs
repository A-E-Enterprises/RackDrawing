using DrawingControl;
using System.Collections.Generic;

namespace RackDrawingApp
{
	public class SheetPreview : BaseViewModel
	{
		public SheetPreview(DrawingSheet sheet)
		{
			m_Sheet = sheet;
		}

		//=============================================================================
		private DrawingSheet m_Sheet = null;
		public DrawingSheet Sheet { get { return m_Sheet; } }

		//=============================================================================
		public bool IsEnabled
		{
			get
			{
				string strErrorMessage = this.ErrorMessage;
				if (strErrorMessage == null)
					return true;
				return false;
			}
		}

		//=============================================================================
		public string ErrorMessage
		{
			get
			{
				if (m_Sheet != null)
				{
					if (m_Sheet.BoundSheetGeometry != null)
						return "This sheet is already used at Warehouse sheet.";

					if (m_Sheet.Document != null)
					{
						WarehouseSheet whSheet = m_Sheet.Document.WarehouseSheet;
						if (whSheet != null)
						{
							if (m_Sheet == whSheet)
								return "Warehouse sheet can be placed at another warehouse sheet.";

							if (Utils.FGT(m_Sheet.Length, whSheet.Length) || Utils.FGT(m_Sheet.Width, whSheet.Width))
								return "Sheet length or width is greater than warehouse sheet size.";
						}
					}
				}

				return null;
			}
		}
	}

	public class PlaceSheetDialogVM : BaseViewModel
	{
		public PlaceSheetDialogVM(DrawingDocument doc)
		{
			// Add "Without bound sheet".
			m_SheetsPreviewsList.Add(new SheetPreview(null));

			// Add sheets from the document.
			if(doc != null && doc.Sheets != null)
			{
				foreach(DrawingSheet sheet in doc.Sheets)
				{
					if (sheet == null)
						continue;

					if (sheet is WarehouseSheet)
						continue;

					m_SheetsPreviewsList.Add(new SheetPreview(sheet));
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// List with sheets which can be placed at Warehouse Sheet.
		/// </summary>
		private List<SheetPreview> m_SheetsPreviewsList = new List<SheetPreview>();
		public List<SheetPreview> SheetsPreviewsList { get { return m_SheetsPreviewsList; } }

		//=============================================================================
		private SheetPreview m_SelectedSheetPreview = null;
		public SheetPreview SelectedSheetPreview
		{
			get { return m_SelectedSheetPreview; }
			set { m_SelectedSheetPreview = value; }
		}
	}
}
