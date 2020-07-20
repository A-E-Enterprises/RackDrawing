using DrawingControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RackDrawingApp
{
	public class PropertyTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DefaultTemplate { get; set; }
		public DataTemplate BooleanTemplate { get; set; }
		public DataTemplate ComboboxTemplate { get; set; }

		//=============================================================================
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			Property_ViewModel _prop = item as Property_ViewModel;
			if(_prop != null)
			{
				List<string> _stValues = _prop.StandardValues;
				if (_stValues != null && _stValues.Count > 0)
					return ComboboxTemplate;

				object _val = _prop.Value;
				if (_val is bool)
					return BooleanTemplate;
			}

			return DefaultTemplate;
		}
	}
}
