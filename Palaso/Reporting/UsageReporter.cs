using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public class UsageReporter
	{
		private static string s_appNameToUseInDialogs;
		private static string s_appNameToUseInReporting;
		private static ReportingSettings s_settings;

		/// <summary>
		/// call this each time the application is launched
		/// </summary>
		public static void RecordLaunch()
		{
		   GetUserIdentifierIfNeeded();

		   //MakeLaunchDateSafe();

//            int launchCount = 1 + int.Parse(RegistryAccess.GetStringRegistryValue("launches", "0"));
//            RegistryAccess.SetStringRegistryValue("launches", launchCount.ToString());
			if (DateTime.UtcNow.Date != s_settings.LastLaunchDate.Date)
			{
				s_settings.LastLaunchDate = DateTime.UtcNow.Date;
				s_settings.Launches++;
				//ReportingSetting.Default.Save();
				AttemptHttpReport();
			}
		}

		private static void GetUserIdentifierIfNeeded( )
		{
			//nb, this tries to share the id between applications that might want it,
			//so the user doesn't have to be asked again.

			string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "SIL");
			Directory.CreateDirectory(dir);
			string path = Path.Combine(dir, "UserIdentifier.txt");
		   // ReportingSetting.Default.Identifier = "";
			if (!s_settings.HaveShowRegistrationDialog)
			{
					s_settings.HaveShowRegistrationDialog = true;
					UserRegistrationDialog dlg = new UserRegistrationDialog();
					if (File.Exists(path))
					{
						s_settings.UserIdentifier = File.ReadAllText(path);
					}
					dlg.ShowDialog();
					s_settings.UserIdentifier = dlg.EmailAddress;
					s_settings.OkToPingBasicUsageData= dlg.OkToCollectBasicStats;

					//ReportingSetting.Default.Save();
					File.WriteAllText(path, s_settings.UserIdentifier);
			}

		}

		/// <summary>
		/// cover an apparent bug in the generated code when you do a get but the datetime is null
		/// </summary>
		private static void MakeLaunchDateSafe( )
		{
			try
			{
				DateTime dummy = s_settings.LastLaunchDate;
			}
			catch
			{
				s_settings.LastLaunchDate = default(DateTime);
			}
		}
/*
		/// <summary>
		/// could be the email, but might not be
		/// </summary>
		public static string UserIdentifier
		{
			get
			{
				return ReportingSetting.Default.UserIdentifier;
			}
			set
			{
				ReportingSetting.Default.UserIdentifier = value;
			}
		}
*/

		public static ReportingSettings AppReportingSettings
		{
			get
			{
				return s_settings;
			}
			set
			{
				s_settings = value;
			}
		}

		public static string AppNameToUseInDialogs
		{
			get
			{
				if (!String.IsNullOrEmpty(s_appNameToUseInReporting))
				{
				 return s_appNameToUseInDialogs;

				}
				return Application.ProductName;
			}
			set
			{
				s_appNameToUseInDialogs = value;
			}
		}

		public static string AppNameToUseInReporting
		{
			get
			{
				if (!String.IsNullOrEmpty(s_appNameToUseInReporting))
				{
					 return s_appNameToUseInReporting;
				}
				return Application.ProductName;
			}
			set
			{
				s_appNameToUseInReporting = value;
			}
		}

		/// <summary>
		/// used for testing purposes
		/// </summary>
		public static void ResetSettingsForTests()
		{
		   // RegistryAccess.SetStringRegistryValue("launches", "0");
			if (s_settings == null)
			{
				s_settings = new ReportingSettings();
			}

			s_settings.Launches=0;
			MakeLaunchDateSafe();
		}


		/// <summary>
		/// if you call this every time the application starts, it will send reports on those intervals
		/// (e.g. {1, 10}) that are listed in the intervals parameter.  It will get version number and name out of the application.
		/// </summary>
		public static void DoTrivialUsageReport(string emailAddress, string topMessage, int[] intervals)
		{
			MakeLaunchDateSafe();

			//avoid asking the user more than once on the special reporting days
			if (DateTime.UtcNow.Date != s_settings.LastLaunchDate.Date)
			{
				foreach (int launch in intervals)
				{
					if (launch == s_settings.Launches)
					{
						SendReport(emailAddress, topMessage);
						break;
					}
				}
			}
		}

		private static void SendReport(string emailAddress, string topMessage)
		{
			// Set the Application label to the name of the app
			Assembly assembly = Assembly.GetEntryAssembly();
			string version = Application.ProductVersion;
			if (assembly != null)
			{
				object[] attributes = assembly.GetCustomAttributes(typeof (AssemblyFileVersionAttribute), false);
				version = (attributes != null && attributes.Length > 0)
							  ?
						  ((AssemblyFileVersionAttribute) attributes[0]).Version
							  : Application.ProductVersion;
			}


			if (!AttemptHttpReport())
			{
				using (UsageEmailDialog d = new UsageEmailDialog())
				{
					d.TopLineText = topMessage;
					d.EmailMessage.To.Add(emailAddress);
					d.EmailMessage.Subject =
						string.Format("{0} {1} Report {2} Launches",
									  UsageReporter.AppNameToUseInReporting,
									  version,
									  s_settings.Launches);
					d.EmailMessage.Body =
						string.Format("app={0} version={1} launches={2}",
									  UsageReporter.AppNameToUseInReporting,
									  version,
									  s_settings.Launches);
					d.ShowDialog();
				}
			}
		}


		public static bool AttemptHttpReport()
		{
			try
			{
				if(!s_settings.OkToPingBasicUsageData)
					return false;

				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("app", UsageReporter.AppNameToUseInReporting);
				parameters.Add("version", ErrorReport.VersionNumberString);
				parameters.Add("launches", s_settings.Launches.ToString());
				parameters.Add("user", s_settings.UserIdentifier);

				string result = HttpPost("http://www.wesay.org/usage/post.php", parameters);
				return result == "OK";
			}
			catch(Exception)
			{
				Reporting.Logger.WriteMinorEvent("Http Report Failed");
				return false;
			}
		}

		private static string HttpPost(string uri, Dictionary<string, string> parameters)
		{
			StringBuilder parameterBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> pair in parameters)
			{
				parameterBuilder.Append(HttpUtility.UrlEncode(pair.Key));
				parameterBuilder.Append("=");
				parameterBuilder.Append(HttpUtility.UrlEncode(pair.Value));
				parameterBuilder.Append("&");
			}
			//trim off the last "&"
			if (parameterBuilder.Length > 0)
			{
				parameterBuilder.Remove(parameterBuilder.Length - 1, 1);
			}

			System.Net.WebRequest req = System.Net.WebRequest.Create(uri);
		   // req.Proxy = new System.Net.WebProxy(ProxyString, true);

			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			req.Timeout = 1000;

			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameterBuilder.ToString());
			req.ContentLength = bytes.Length;

			System.IO.Stream os = req.GetRequestStream();
			os.Write(bytes, 0, bytes.Length);
			os.Close();

			System.Net.WebResponse resp = req.GetResponse();
			if (resp == null) return null;

			System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
			return sr.ReadToEnd().Trim();
		}
	}
}
