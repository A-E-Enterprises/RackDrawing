using System;
using System.IO;
using System.Net;
using LicenseServer;
using CommonUtilities;

namespace RackDrawingApp_LicenseGeneratorServer
{
	class LicenseGeneratorServer
	{
		static void Main(string[] args)
		{
			// WARNING!!!
			// Application should have administrator rights otherwise "listener.Start();" throws an exception.
			try
			{
				HttpListener listener = new HttpListener();
				listener.Prefixes.Add(LicenseServerData.LICENSE_GENERATOR_SERVER_URI);
				listener.Start();
				Console.WriteLine("Waiting for connections...");

				long connectionsCount = 0;
				while (true)
				{
					++connectionsCount;

					HttpListenerContext context = listener.GetContext();
					HttpListenerRequest request = context.Request;
					HttpListenerResponse response = context.Response;

					// Display request and parse parameters.
					Console.WriteLine("\nConnection #{0}\nQuery: {1}", connectionsCount, request.Url);
					string strUsername = string.Empty;
					string strPassword = string.Empty;
					string strEthAddr = string.Empty;
					string strGUID = string.Empty;
					string strEndDate = string.Empty;
					string strPlatformID = string.Empty;
					string strWinMajor = string.Empty;
					string strWinMinor = string.Empty;
					string strExcelVersion = string.Empty;
					string strPath = string.Empty;
					foreach (var queryKey in context.Request.QueryString.Keys)
					{
						if (queryKey == null)
							continue;

						string strQueryKey = queryKey.ToString();
						if (string.IsNullOrEmpty(strQueryKey))
							continue;

						object queryValue = context.Request.QueryString[strQueryKey];
						string strQueryValue = string.Empty;
						if (queryValue != null)
							strQueryValue = queryValue.ToString();

						Console.WriteLine("{0} = {1}", strQueryKey, strQueryValue);

						if (strQueryKey == LicenseServerData.PARAM_USERNAME)
							strUsername = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_PASSWORD)
							strPassword = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_ETHERNET_ADDRESS)
							strEthAddr = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_GUID)
							strGUID = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_END_DATE)
							strEndDate = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_PLATFORM_ID)
							strPlatformID = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_WINVER_MAJOR)
							strWinMajor = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_WINVER_MINOR)
							strWinMinor = strQueryValue;
						else if (strQueryKey == LicenseServerData.PARAM_EXCEL_VERSION)
							strExcelVersion = strQueryValue;
					}

					// Try to create license.
					LicenseData licenseData = new LicenseData();
					licenseData.Username = strUsername;
					licenseData.Password = strPassword;
					licenseData.EthernetAddress = strEthAddr;
					licenseData.GUID = strGUID;
					//
					if (string.IsNullOrEmpty(strEndDate))
					{
						licenseData.IncludeDate = false;
					}
					else
					{
						licenseData.IncludeDate = true;
						try
						{
							licenseData.CanRunTill = Convert.ToDateTime(strEndDate);
						}
						catch(Exception ex)
						{
							response.StatusCode = (int)HttpStatusCode.BadRequest;
							response.StatusDescription = "Incorrect parameters.";
							response.OutputStream.Close();

							Console.WriteLine("ERROR, cant parse \"end_date\" parameter: " + ex.Message);
							continue;
						}
					}
					//
					licenseData.PlatformID = strPlatformID;
					licenseData.WindowsVersionMajor = strWinMajor;
					licenseData.WindowsVersionMinor = strWinMinor;
					licenseData.ExcelVersion = strExcelVersion;

					// Try to create license file in the assembly directory.
					string assemblyFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
					string licenseFilePath = assemblyFolder + "\\RackDrawingAppLicense.lic";
					string strError;
					bool bError = !LicenseUtilities.sCreateLicense(licenseData, licenseFilePath, out strError);

					// Send license file in response.
					if (!bError)
					{
						using (FileStream fs = File.OpenRead(licenseFilePath))
						{
							string filename = Path.GetFileName(licenseFilePath);
							//response is HttpListenerContext.Response...
							response.ContentLength64 = fs.Length;
							response.SendChunked = false;
							response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
							response.AddHeader("Content-disposition", "attachment; filename=" + filename);

							byte[] buffer = new byte[64 * 1024];
							int read;
							using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
							{
								while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
								{
									bw.Write(buffer, 0, read);
									bw.Flush(); //seems to have no effect
								}

								bw.Close();
							}

							response.StatusCode = (int)HttpStatusCode.OK;
							response.StatusDescription = "OK";
							response.OutputStream.Close();
						}

						Console.WriteLine("SUCCESS. License file was sent.");
					}
					else
					{
						response.StatusCode = (int)HttpStatusCode.BadRequest;
						response.StatusDescription = "Incorrect parameters.";
						response.OutputStream.Close();

						Console.WriteLine("ERROR. " + strError);
					}

					File.Delete(licenseFilePath);
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("ERROR: " + ex.Message);
				Console.WriteLine("Press any key...");
				Console.ReadKey();
			}
		}
	}
}
