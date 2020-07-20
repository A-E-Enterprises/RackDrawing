using DrawingControl;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace RackDrawingApp
{
	/// <summary>
	/// Wraps BaseRectangleGeometry 3D models.
	/// Apply transform matrix to the visual.
	/// </summary>
	public class GeometryVisual3D : ModelVisual3D
	{
		protected GeometryVisual3D()
		{
			//     | Z
			//     |
			//     |
			//     | _________ X
			//    /
			//   /
			//  / Y
			Matrix3D transformMatrix = new Matrix3D(
				0, 1, 0, 0,
				1, 0, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1
				);

			this.Transform = new MatrixTransform3D(transformMatrix);
		}
		public GeometryVisual3D(List<Model3D> geometryModelsList)
			: this()
		{
			if(geometryModelsList != null)
			{
				Model3DGroup geometryModelsGroup = new Model3DGroup();
				foreach(Model3D model in geometryModelsList)
				{
					if (model == null)
						continue;

					geometryModelsGroup.Children.Add(model);
				}

				this.Content = geometryModelsGroup;
			}
		}
	}
}
