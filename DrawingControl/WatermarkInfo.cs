namespace DrawingControl
{
	/// <summary>
	/// Static class which drives watermark display properties in the application:
	/// sheet layout, rack advanced properties picture, pdf and image export.
	/// </summary>
	public static class WatermarkInfo
	{
		/// <summary>
		/// Angle of watermark in degrees.
		/// </summary>
		public static double WatermarkAngle { get { return 0.0; } }

		/// <summary>
		/// Number of rows and columns with watermark picture.
		/// Watermark picture will be automatically stretched up\down to fill available space and
		/// keep the same watermark picture length\height ratio.
		/// </summary>
		public static uint Rows { get { return 4; } }
		public static uint Columns { get { return 2; } }

		/// <summary>
		/// Margins between watermark images.
		/// Warning! Margin applied on the both sides of image.
		/// For example, if you set 100 as MarginX then 100 will be applied to the left and right side of watermark picture.
		/// </summary>
		public static uint MarginX { get { return 100; } }
		public static uint MarginY { get { return 100; } }

		/// <summary>
		/// Watermark image opacity. It is number from 0 to 1.
		/// 0 - full transparent image
		/// 1 - not transparent at all
		/// </summary>
		private static double m_Opacity = 0.1;
		public static double Opacity
		{
			get { return m_Opacity; }
			set
			{
				if (Utils.FGE(value, 0.0))
					m_Opacity = value;
			}
		}
	}
}
