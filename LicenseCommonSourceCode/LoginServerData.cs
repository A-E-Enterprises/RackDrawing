namespace LoginServer
{
	public static class LoginServerData
	{
		// URI of login server. RackDrawingApp sends license file request to this server.
		public static string LOGIN_SERVER_URI = "http://103.81.243.66"; //"http://localhost:7144/";
		// Parameters for GET request to the login server.
		public static string PARAM_USERNAME = "username";
		public static string PARAM_PASSWORD = "password";
	}
}