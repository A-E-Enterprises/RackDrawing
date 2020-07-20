using DrawingControl;
using System.Text;

namespace RackDrawingApp
{
	public class EditSheetNotesDailogViewModel : BaseViewModel
	{
		public EditSheetNotesDailogViewModel(DrawingSheet sheet)
		{
			if (sheet != null)
				Notes = sheet.Notes;
		}

		#region Properties

		/// <summary>
		/// Max symbols count for Notes text
		/// </summary>
		private uint m_MaxSymbolsCount = 300;

		/// <summary>
		/// Text with total and max symbols count.
		/// </summary>
		public string TotalSymbolsCount
		{
			get
			{
				StringBuilder sb = new StringBuilder("Total symbols count: ");
				sb.Append(m_Notes.Length.ToString());
				sb.Append("(");
				sb.Append(m_MaxSymbolsCount.ToString());
				sb.Append(")");

				return sb.ToString();
			}
		}

		/// <summary>
		/// Notes text
		/// </summary>
		private string m_Notes = string.Empty;
		public string Notes
		{
			get { return m_Notes; }
			set
			{
				m_Notes = value;
				if (m_Notes == null)
					m_Notes = string.Empty;
				NotifyPropertyChanged(() => Notes);
				NotifyPropertyChanged(() => TotalSymbolsCount);
			}
		}

		#endregion
	}
}
