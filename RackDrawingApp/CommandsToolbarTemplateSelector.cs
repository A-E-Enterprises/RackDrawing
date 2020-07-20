using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RackDrawingApp
{
	/// <summary>
	/// Selects template for the WPF-command
	/// </summary>
	public class CommandsToolbarTemplateSelector : DataTemplateSelector
	{
		/// <summary>
		/// Button with picture, which calls command on click
		/// </summary>
		public DataTemplate CmdButtonDataTemplate { get; set; }
		/// <summary>
		/// Separator
		/// </summary>
		public DataTemplate SeparatorDataTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item != null)
				return CmdButtonDataTemplate;

			return SeparatorDataTemplate;
		}
	}
}
