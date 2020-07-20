using DrawingControl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace RackDrawingApp
{
	// Converts Pallet to list with available commands for this pallet.
	// Used in pallet properties list view at the rack advanced properties tab.
	public class PalletCommandsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Pallet pallet = value as Pallet;
			if (pallet == null)
				return null;

			if (pallet.Level == null
				|| pallet.Level.Owner == null
				|| pallet.Level.Owner.Sheet == null
				|| pallet.Level.Owner.Sheet.Document == null
				|| pallet.Level.Owner.Sheet.Document.PalletConfigurationCollection == null)
				return null;

			List<ICommand> commands = new List<ICommand>();
			if (pallet.PalletConfiguration != null)
				commands.Add(new Command_UnbindPalletConfiguration());
			else if(!pallet.Level.Owner.Sheet.Document.ContainsPalletConfig(pallet.Length, pallet.Width, pallet.Height, pallet.Load))
				commands.Add(new Command_SavePalletConfiguration());

			return commands;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
