namespace RackDrawingApp
{
	/// <summary>
	/// Data passed as parameters on application start.
	/// </summary>
	public static class UserInfo
	{
		/// <summary>
		/// Login string passed through the parameters or used at LoginWindow.
		/// </summary>
		private static string m_Login = string.Empty;
		public static string Login
		{
			get { return m_Login; }
			set { m_Login = value; }
		}
		//
		private static string m_CustomerName = string.Empty;
		public static string CustomerName
		{
			get { return m_CustomerName; }
			set { m_CustomerName = value; }
		}
		//
		private static string m_EnqNo = string.Empty;
		public static string EnqNo
		{
			get { return m_EnqNo; }
			set { m_EnqNo = value; }
		}
		//
		private static string m_CustomerContactNo = string.Empty;
		public static string CustomerContactNo
		{
			get { return m_CustomerContactNo; }
			set { m_CustomerContactNo = value; }
		}
		//
		private static string m_CustomerEmailID = string.Empty;
		public static string CustomerEmailID
		{
			get { return m_CustomerEmailID; }
			set { m_CustomerEmailID = value; }
		}
		//
		private static string m_CustomerBillingAddress = string.Empty;
		public static string CustomerBillingAddress
		{
			get { return m_CustomerBillingAddress; }
			set { m_CustomerBillingAddress = value; }
		}
		//
		private static string m_CustomerSiteAddress = string.Empty;
		public static string CustomerSiteAddress
		{
			get { return m_CustomerSiteAddress; }
			set { m_CustomerSiteAddress = value; }
		}

		/// <summary>
		/// Drawing path which was passed through command line arguments.
		/// </summary>
		private static string m_DrawingPath = string.Empty;
		public static string DrawingPath
		{
			get { return m_DrawingPath; }
			set { m_DrawingPath = value; }
		}
	}
}
