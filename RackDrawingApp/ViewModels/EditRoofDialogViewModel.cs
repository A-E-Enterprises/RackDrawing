using DrawingControl;
using System.Collections.Generic;

namespace RackDrawingApp
{
	public class EditRoofDialogViewModel : BaseViewModel
	{
		public EditRoofDialogViewModel(DrawingSheet sheet)
		{
			m_Sheet = sheet;
		}

		//=============================================================================
		// Sheet with roofs.
		private DrawingSheet m_Sheet = null;
		public DrawingSheet Sheet { get { return m_Sheet; } }

		//=============================================================================
		// List with direction values for GableRoof type.
		// GableRoof has bool flag as this value, but need to display "horizontal"
		// and "vertical" strings for user instead bool value;
		private List<string> m_GableRoofDirectionValuesList = new List<string>()
		{
			GableRoofDirectionConverter.STR_GABLEROOF_HORIZONTAL,
			GableRoofDirectionConverter.STR_GABLEROOF_VERTICAL
		};
		public List<string> GableRoofDirectionValuesList { get { return m_GableRoofDirectionValuesList; } }

		//=============================================================================
		// List with direction values for ShedRoof type.
		// PitchDirection property has ePitchDirection type, but need to display strings for user.
		// This string list is used as ItemSource for the combobox.
		private List<string> m_ShedRoofDirectionValuesList = new List<string>()
		{
			ShedRoofDirectionConverter.STR_SHEDROOF_LEFT_TO_RIGHT,
			ShedRoofDirectionConverter.STR_SHEDROOF_RIGHT_TO_LEFT,
			ShedRoofDirectionConverter.STR_SHEDROOF_TOP_TO_BOTTOM,
			ShedRoofDirectionConverter.STR_SHEDROOF_BOTTOM_TO_TOP
		};
		public List<string> ShedRoofDirectionValuesList { get { return m_ShedRoofDirectionValuesList; } }
	}
}
