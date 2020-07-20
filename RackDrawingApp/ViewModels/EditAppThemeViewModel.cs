using AppColorTheme;
using DrawingControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RackDrawingApp
{
	/// <summary>
	/// View model wrapped over color
	/// </summary>
	public abstract class ColorViewModel : BaseViewModel
	{
		public ColorViewModel() { }

		#region Properties

		/// <summary>
		/// Color display name
		/// </summary>
		public abstract string DisplayName { get; }

		/// <summary>
		/// Color desciption
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Color value
		/// </summary>
		public abstract Color Value { get; set; }

		/// <summary>
		/// Brush with color value
		/// </summary>
		public Brush ColorBrush { get { return new SolidColorBrush(Value); } }

		#endregion
	}

	/// <summary>
	/// Wraps ColorInfo
	/// </summary>
	public class ColorInfoViewModel : ColorViewModel
	{
		public ColorInfoViewModel(ColorInfo colorInfo)
		{
			m_ColorInfo = colorInfo;
		}

		#region Properties

		private ColorInfo m_ColorInfo = null;

		/// <summary>
		/// Color display name
		/// </summary>
		public override string DisplayName
		{
			get
			{
				if (m_ColorInfo != null)
				{
					if (!string.IsNullOrEmpty(m_ColorInfo.LocalName))
						return m_ColorInfo.LocalName;

					return m_ColorInfo.SystemName;
				}

				return "(Empty SystemName)";
			}
		}

		/// <summary>
		/// Color desciption
		/// </summary>
		public override string Description
		{
			get
			{
				if (m_ColorInfo != null)
					return m_ColorInfo.Description;

				return string.Empty;
			}
		}

		/// <summary>
		/// Color value
		/// </summary>
		public override Color Value
		{
			get
			{
				if (m_ColorInfo != null)
					return m_ColorInfo.Value;

				return Colors.Black;
			}
			set
			{
				if (m_ColorInfo != null)
					m_ColorInfo.Value = value;

				NotifyPropertyChanged(() => Value);
				NotifyPropertyChanged(() => ColorBrush);
			}
		}

		#endregion
	}

	/// <summary>
	/// Wraps geometry color
	/// </summary>
	public class GeomColorViewModel : ColorViewModel
	{
		public GeomColorViewModel(eColorType colorType, IGeometryColorsTheme geomColorTheme)
		{
			m_ColorType = colorType;
			m_ParentGeomColorTheme = geomColorTheme;
		}

		#region Properties

		private eColorType m_ColorType = eColorType.eUndefined;
		private IGeometryColorsTheme m_ParentGeomColorTheme = new DefaultGeometryColorsTheme();

		/// <summary>
		/// Color display name
		/// </summary>
		public override string DisplayName
		{
			get
			{
				string strLocalName = ColorTheme.ColorTypeToLocalName(m_ColorType);
				if (!string.IsNullOrEmpty(strLocalName))
					return strLocalName;

				return ColorTheme.ColorTypeToSystemName(m_ColorType);
			}
		}

		/// <summary>
		/// Color desciption
		/// </summary>
		public override string Description { get { return ColorTheme.ColorTypeToDescription(m_ColorType); } }

		/// <summary>
		/// Color value
		/// </summary>
		public override Color Value
		{
			get
			{
				if(m_ParentGeomColorTheme != null)
				{
					Color colorValue;
					if (m_ParentGeomColorTheme.GetGeometryColor(m_ColorType, out colorValue))
						return colorValue;
				}

				return Colors.Black;
			}
			set
			{
				if (m_ParentGeomColorTheme != null)
					m_ParentGeomColorTheme.SetGeometryColor(m_ColorType, value);

				NotifyPropertyChanged(() => Value);
				NotifyPropertyChanged(() => ColorBrush);
			}
		}

		#endregion
	}

	public class EditAppThemeViewModel : BaseViewModel
	{
		public EditAppThemeViewModel(ColorTheme colorTheme)
		{
			m_ColorTheme = colorTheme;
			UpdateCollections();
		}

		#region Properties

		/// <summary>
		/// Color theme which is edited through dialog.
		/// </summary>
		private ColorTheme m_ColorTheme = null;
		public ColorTheme ColorTheme
		{
			get { return m_ColorTheme; }
			set
			{
				m_ColorTheme = value;
				UpdateCollections();
				NotifyPropertyChanged(() => ColorTheme);
			}
		}

		/// <summary>
		/// Collection with interface colors
		/// </summary>
		private ObservableCollection<ColorViewModel> m_InterfaceColorsCollection = new ObservableCollection<ColorViewModel>();
		public ObservableCollection<ColorViewModel> InterfaceColorsCollection { get { return m_InterfaceColorsCollection; } }

		/// <summary>
		/// Collection with geometry colors
		/// </summary>
		private ObservableCollection<ColorViewModel> m_GeometryColorsCollection = new ObservableCollection<ColorViewModel>();
		public ObservableCollection<ColorViewModel> GeometryColorsCollection { get { return m_GeometryColorsCollection; } }

		/// <summary>
		/// Collection with bottom toolbar commands.
		/// </summary>
		private ObservableCollection<Command> m_BottomToolbarCommandsCollection = new ObservableCollection<Command>()
		{
			new Command_OpenTheme(),
			new Command_SaveTheme()
		};
		public ObservableCollection<Command> BottomToolbarCommandsCollection { get { return m_BottomToolbarCommandsCollection; } }

		#endregion

		#region Methods

		/// <summary>
		/// Update colors collections - m_InterfaceColorsCollection and m_GeometryColorsCollection
		/// </summary>
		private void UpdateCollections()
		{
			m_InterfaceColorsCollection.Clear();
			m_GeometryColorsCollection.Clear();

			if (m_ColorTheme == null)
				return;

			List<ColorInfo> interfaceColorsList = m_ColorTheme.InterfaceColorsList;
			if(interfaceColorsList != null)
			{
				foreach(ColorInfo colorInfo in interfaceColorsList)
				{
					if (colorInfo == null)
						continue;

					m_InterfaceColorsCollection.Add(new ColorInfoViewModel(colorInfo));
				}
			}

			if (m_ColorTheme.GeometryColorsTheme != null)
			{
				foreach (eColorType colorType in (eColorType[])Enum.GetValues(typeof(eColorType)))
				{
					if (eColorType.eUndefined == colorType)
						continue;

					GeomColorViewModel geomColorVM = new GeomColorViewModel(colorType, m_ColorTheme.GeometryColorsTheme);
					m_GeometryColorsCollection.Add(geomColorVM);
				}
			}
		}

		#endregion
	}
}
