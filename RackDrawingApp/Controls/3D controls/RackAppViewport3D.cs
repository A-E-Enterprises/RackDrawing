using DrawingControl;
using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Collections.Generic;

namespace RackDrawingApp
{
	/// <summary>
	/// WPF-control which displays single rack or sheet 3D view.
	/// </summary>
	public class RackAppViewport3D : HelixViewport3D
	{
		private static List<RackAppViewport3D> m_ViewportsList = new List<RackAppViewport3D>();

		/// <summary>
		/// Drives RackAppViewport3D content.
		/// </summary>
		public enum eViewportContent : int
		{
			/// <summary>
			/// Displays single selected rack 3D view
			/// </summary>
			eSelectedRack = 1,
			/// <summary>
			/// Displays selected sheet 3D view
			/// </summary>
			eSelectedSheet = 2
		}

		public RackAppViewport3D()
		{
			this.ShowCoordinateSystem = true;
			this.CoordinateSystemLabelX = "Y";
			this.CoordinateSystemLabelY = "X";
			this.ZoomExtentsWhenLoaded = true;

			// init camera
			PerspectiveCamera camera = new PerspectiveCamera();
			camera.Position = new Point3D(1, 1, 1);
			camera.LookDirection = new Vector3D(-1, -1, -1);
			camera.UpDirection = new Vector3D(0, 0, 1);
			this.Camera = camera;

			m_ViewportsList.Add(this);

			//
			m_WatermarkVisual = new WatermarkVisual(this);
			this.AddLogicalChild(m_WatermarkVisual);
			this.AddVisualChild(m_WatermarkVisual);

			this.IsVisibleChanged += RackViewport3D_IsVisibleChanged;
		}

		#region Dependency properties

		//=============================================================================
		/// <summary>
		/// Sheet, selected rack on which will be displayed in 3D view.
		/// </summary>
		public static readonly DependencyProperty SheetProperty = DependencyProperty.Register(
			"Sheet",
			typeof(DrawingSheet),
			typeof(RackAppViewport3D),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, On_Sheet_Changed));
		public DrawingSheet Sheet
		{
			get { return (DrawingSheet)GetValue(RackAppViewport3D.SheetProperty); }
			set { SetValue(RackAppViewport3D.SheetProperty, value); }
		}
		private static void On_Sheet_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingSheet oldSheet = e.OldValue as DrawingSheet;
			if (oldSheet != null)
				oldSheet.PropertyChanged -= SheetPropertyChanged;

			DrawingSheet newSheet = e.NewValue as DrawingSheet;
			if(newSheet != null)
				newSheet.PropertyChanged += SheetPropertyChanged;

			RackAppViewport3D rackViewport = d as RackAppViewport3D;
			if (rackViewport != null)
			{
				// Zoom to fit all content when user changes sheet.
				rackViewport.m_bZoomExtents = true;
				rackViewport.UpdateGeometry();
			}
		}
		//
		/// <summary>
		/// Update 3D view if sheet property was changed.
		/// </summary>
		private static void SheetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			DrawingSheet sheet = sender as DrawingSheet;
			if (sheet == null)
				return;

			if(e.PropertyName == Utils.GetPropertyName(() => sheet.SingleSelectedGeometry))
			{
				foreach(RackAppViewport3D vp in m_ViewportsList)
				{
					if (vp == null)
						continue;

					vp.UpdateGeometry();
					vp.InvalidateVisual();
				}
			}
		}

		//=============================================================================
		/// <summary>
		/// Drives content of the RackAppViewport3D.
		/// Should it be a single rack or all sheet.
		/// </summary>
		public static readonly DependencyProperty ViewportContentProperty = DependencyProperty.Register(
			"ViewportContent",
			typeof(eViewportContent),
			typeof(RackAppViewport3D),
			new FrameworkPropertyMetadata(eViewportContent.eSelectedRack, FrameworkPropertyMetadataOptions.AffectsRender, On_ViewportContent_Changed)
			);
		public eViewportContent ViewportContent
		{
			get { return (eViewportContent)GetValue(RackAppViewport3D.ViewportContentProperty); }
			set { SetValue(RackAppViewport3D.ViewportContentProperty, value); }
		}
		private static void On_ViewportContent_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RackAppViewport3D rackViewport = d as RackAppViewport3D;
			if (rackViewport != null)
			{
				// Zoom to fit all content when user changes viewport content type.
				rackViewport.m_bZoomExtents = true;
				rackViewport.UpdateGeometry();
			}
		}

		//=============================================================================
		/// <summary>
		/// If true then walls will be displayed at the 3D picture.
		/// </summary>
		public static readonly DependencyProperty ShowWallsProperty = DependencyProperty.Register(
			"ShowWalls",
			typeof(bool),
			typeof(RackAppViewport3D),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, On_ShowWalls_Changed)
			);
		public bool ShowWalls
		{
			get { return (bool)GetValue(RackAppViewport3D.ShowWallsProperty); }
			set { SetValue(RackAppViewport3D.ShowWallsProperty, value); }
		}
		private static void On_ShowWalls_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RackAppViewport3D rackViewport = d as RackAppViewport3D;
			if (rackViewport != null)
				rackViewport.UpdateGeometry();
		}

		//=============================================================================
		public static readonly DependencyProperty ShowRoofProperty = DependencyProperty.Register(
			"ShowRoof",
			typeof(bool),
			typeof(RackAppViewport3D),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, On_ShowRoof_Changed)
			);
		public bool ShowRoof
		{
			get { return (bool)GetValue(RackAppViewport3D.ShowRoofProperty); }
			set { SetValue(RackAppViewport3D.ShowRoofProperty, value); }
		}
		private static void On_ShowRoof_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RackAppViewport3D rackViewport = d as RackAppViewport3D;
			if (rackViewport != null)
				rackViewport.UpdateGeometry();
		}

		#endregion

		#region Properties

		//=============================================================================
		/// <summary>
		/// Dispays watermark over control.
		/// </summary>
		private WatermarkVisual m_WatermarkVisual = null;

		//=============================================================================
		/// <summary>
		/// Need to apply zoom to the camera when any rack is displayed for the first time.
		/// Otherwise, it is need to make a lot of mouse wheel spins for user.
		/// Need to apply zoom only once, otherwise camera poisiton will be reset every time when
		/// control is displayed, sheet or property changed.
		/// </summary>
		private bool m_bZoomExtents = true;

		#endregion

		#region Functions overrides

		//=============================================================================
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Draw watermark
			if (m_WatermarkVisual != null && WatermarkVisual.WatermarkImage != null)
				m_WatermarkVisual.Draw(WatermarkVisual.WatermarkImage);
		}

		//=============================================================================
		protected override int VisualChildrenCount
		{
			get
			{
				int count = base.VisualChildrenCount;
				// m_WatermarkVisual
				++count;

				return count;
			}
		}
		//=============================================================================
		protected override Visual GetVisualChild(int index)
		{
			if (index == this.VisualChildrenCount - 1)
				return m_WatermarkVisual;

			return base.GetVisualChild(index);
		}

		#endregion

		#region Public functions

		//=============================================================================
		/// <summary>
		/// Clear and rebuild children geometry.
		/// </summary>
		public void UpdateGeometry()
		{
			if (this.Visibility != Visibility.Visible)
				return;

			if (!this.IsVisible)
				return;

			Factory3D.ResetBuilders();

			// Clear chidlren
			this.Children.Clear();

			// Create light
			ModelVisual3D ambLightVisualModel = new ModelVisual3D();
			ambLightVisualModel.Content = new AmbientLight(Color.FromRgb((byte)40, (byte)40, (byte)40));
			this.Children.Add(ambLightVisualModel);
			//
			this.Children.Add(new DefaultLights());

			// Add geometry visual.
			DrawingSheet sheet = this.Sheet;
			if (sheet != null)
			{
				if (eViewportContent.eSelectedRack == this.ViewportContent)
				{
					Factory3D.sDrawRackBeamsColumnsWithRackColor = false;

					// Create rack model
					Rack rack = sheet.SingleSelectedGeometry as Rack;
					if (rack != null)
					{
						Factory3D.AddGeometryVisual(rack, new Point3D(0.0, 0.0, 0.0), false, new Vector(0.0, 0.0), false);
					}
				}
				else if(eViewportContent.eSelectedSheet == this.ViewportContent)
				{
					Factory3D.sDrawRackBeamsColumnsWithRackColor = true;

					// Create sheet model
					Factory3D.AddSheetModel(sheet, new Point3D(0.0, 0.0, 0.0), sheet is WarehouseSheet, this.ShowWalls, true);

					// Add roof for WarehouseSheet
					if (this.ShowRoof)
					{
						WarehouseSheet whSheet = sheet as WarehouseSheet;
						if (whSheet != null)
							Factory3D.AddRoof(whSheet);
					}
				}
			}

			List<Model3D> modelsList = Factory3D.GetModelsList();
			GeometryVisual3D contentVisual = new GeometryVisual3D(modelsList);
			this.Children.Add(contentVisual);

			//
			if (m_bZoomExtents)
			{
				this.UpdateLayout();
				if (this.CameraController != null)
				{
					this.CameraController.ZoomExtents();
					m_bZoomExtents = false;
				}
			}
		}

		#endregion

		#region Protected and private functions

		//=============================================================================
		private void RackViewport3D_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			UpdateGeometry();
		}

		#endregion
	}
}
