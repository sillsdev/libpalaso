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

		/// <summary>
		/// call this each time the application is launched if you have launch count-based reporting
		/// </summary>
		public static void RecordLaunch()
		{
		   GetUserIdentifierIfNeeded();

		   MakeLaunchDateSafe();

//            int launchCount = 1 + int.Parse(RegistryAccess.GetStringRegistryValue("launches", "0"));
//            RegistryAccess.SetStringRegistryValue("launches", launchCount.ToString());
			if (DateTime.UtcNow.Date != ReportingSetting.Default.LastLaunchDate.Date)
			{
				ReportingSetting.Default.LastLaunchDate = DateTime.UtcNow.Date;
				ReportingSetting.Default.Launches++;
				ReportingSetting.Default.Save();
				AttemptHttpReport();
			}
		}

		private static void GetUserIdentifierIfNeeded()
		{
			//nb, this tries to share the id between applications that might want it,
			//so the user doesn't have to be asked again.

			string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "SIL");
			Directory.CreateDirectory(dir);
			string path = Path.Combine(dir, "UserIdentifier.txt");
		   // ReportingSetting.Default.Identifier = "";
			if (string.IsNullOrEmpty(ReportingSetting.Default.UserIdentifier))
			{
				if (File.Exists(path))
				{
					ReportingSetting.Default.UserIdentifier = File.ReadAllText(path);
				}
				else
				{
					UserRegistrationDialog dlg = new UserRegistrationDialog();
					dlg.ShowDialog();
					ReportingSetting.Default.UserIdentifier = dlg.EmailAddress;
					ReportingSetting.Default.Save();
					File.WriteAllText(path, ReportingSetting.Default.UserIdentifier);
				}
			}

		}

		/// <summary>
		/// cover an apparent bug in the generated code when you do a get but the datetime is null
		/// </summary>
		private static void MakeLaunchDateSafe()
		{
			try
			{
				DateTime dummy = ReportingSetting.Default.LastLaunchDate;
			}
			catch
			{
				ReportingSetting.Default.LastLaunchDate = default(DateTime);
			}
		}

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
			ReportingSetting.Default.Launches=0;
			ReportingSetting.Default.Reset();
			MakeLaunchDateSafe();
		}


		/// <summary>
		/// if you call this every time the application starts, it will send reports on those intervals
		/// (e.g. {1, 10}) that are listed in the intervals parameter.  It will get version number and name out of the application.
		/// </summary>
		public static void DoTrivialUsageReport(string emailAddress, string topMessage, int[] intervals)
		{
			MakeLaunchDateSafe();

			foreach (int launch in intervals)
			{
				if (launch == ReportingSetting.Default.Launches)
				{
					SendReport(emailAddress, topMessage);
					break;
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
					d.EmailMessage.Address = emailAddress;
					d.EmailMessage.Subject =
						string.Format("{0} {1} Report {2} Launches", UsageReporter.AppNameToUseInReporting, version,
									  ReportingSetting.Default.Launches);
					d.EmailMessage.Body =
						string.Format("app={0} version={1} launches={2}", UsageReporter.AppNameToUseInReporting, version,
									  ReportingSetting.Default.Launches);
					d.ShowDialog();
				}
			}
		}


		public static bool AttemptHttpReport()
		{
			try
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("app", UsageReporter.AppNameToUseInReporting);
				parameters.Add("version", ErrorReport.VersionNumberString);
				parameters.Add("launches", ReportingSetting.Default.Launches.ToString());

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
