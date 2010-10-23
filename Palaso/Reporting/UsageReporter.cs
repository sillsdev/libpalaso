using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Palaso.Code;
using Palaso.Network;

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
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");

		   GetUserIdentifierIfNeeded();

			 if (DateTime.UtcNow.Date != s_settings.LastLaunchDate.Date)
			{
				s_settings.LastLaunchDate = DateTime.UtcNow.Date;
				s_settings.Launches++;
				AttemptHttpReport();
			}
		}

		private static void GetUserIdentifierIfNeeded( )
		{
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");

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

					//NB: the current system requires that the caller do the saving
					//of the other settings.

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

		/// <summary>
		/// The deal here is, Cambell changed this so that it is the responsibility of
		/// the client to set this, and then save the settings.
		/// E.g., in the Settings.Designer.cs, add
		///
		///     [UserScopedSetting()]
		///		[DebuggerNonUserCode()]
		///		public Palaso.Reporting.ReportingSettings Reporting
		///		{
		///			get { return ((Palaso.Reporting.ReportingSettings)(this["Reporting"])); }
		///			set { this["Reporting"] = value; }
		///		}
		/// </summary>

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
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");

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
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");

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


		public static void ReportLaunchesAsync()
		{
			var worker = new BackgroundWorker();
			worker.DoWork += new DoWorkEventHandler(OnReportDoWork);
			worker.RunWorkerAsync();
		}

		static void OnReportDoWork(object sender, DoWorkEventArgs e)
		{
			Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("app", UsageReporter.AppNameToUseInReporting);
			parameters.Add("version", ErrorReport.VersionNumberString);
			UsageMemory.Default.Launches++;
			parameters.Add("launches", UsageMemory.Default.Launches.ToString());
			UsageMemory.Default.Save();

			try
			{
				string result = HttpPost("http://www.wesay.org/usage/post.php", parameters);
			}
			catch (Exception)
			{
				//so many things can go wrong, but we can't do anything about any of them
			}
		}


		public static string HttpPost(string uri, Dictionary<string, string> parameters)
		{
			try
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

				byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameterBuilder.ToString());

				var client = new WebClient();
				client.Credentials = CredentialCache.DefaultNetworkCredentials;
				client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

				var response = new byte[] { };

				RobustNetworkOperation.Do(proxy =>
				{
					client.Proxy = proxy;
					response = client.UploadData(uri, bytes);
				});


				return System.Text.Encoding.ASCII.GetString(response);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
