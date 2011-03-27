using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
		private AnalyticsEventSender _analytics;
		private string _realPreviousVersion;
		private string _mostRecentArea;

		public UsageReporter(ReportingSettings settings)
		{
			s_settings = settings;
			GetUserIdentifierIfNeeded();
			_realPreviousVersion = s_settings.PreviousVersion;
			s_settings.PreviousVersion = ErrorReport.VersionNumberString;
		}
		/// <summary>
		/// call this each time the application is launched
		/// </summary>
		[Obsolete("Use RecordLaunchAsync, instead")]
		public static void RecordLaunch()
		{
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");

		   GetUserIdentifierIfNeeded();

			 if (DateTime.UtcNow.Date != s_settings.PreviousLaunchDate.Date)
			{
				s_settings.PreviousLaunchDate = DateTime.UtcNow.Date;
				s_settings.Launches++;

				AttemptHttpReport();
			}
		}

		/// <summary>
		/// A unique guid for this machine, which is the same for all palaso apps (because we store it in special palaso text file in appdata)
		/// </summary>
		public static Guid UserGuid
		{
			get
			{
				try
				{
					string guid;
					if (GetAllApplicationValuesForThisUser().TryGetValue("guid", out guid))
						return new Guid(guid);
					else
					{
						Debug.Fail("Why wasn't there a guid already in the values for this user?");
					}
				}
				catch (Exception)
				{
					//ah well.  Debug mode only, we tell the programmer. Otherwise, we're giving a random guid
					Debug.Fail("couldn't parse the user indentifier into a guid");
				}
				return new Guid(); //outside of debug mode, we guarantee some guid is returned... it's only for reporting after all
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
				DateTime dummy = s_settings.PreviousLaunchDate;
			}
			catch
			{
				s_settings.PreviousLaunchDate = default(DateTime);
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
		[Obsolete("Use BeginGoogleAnalytics Instead ")]
		public static void DoTrivialUsageReport(string emailAddress, string topMessage, int[] intervals)
		{
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");

			MakeLaunchDateSafe();

			//avoid asking the user more than once on the special reporting days
			if (DateTime.UtcNow.Date != s_settings.PreviousLaunchDate.Date)
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

				#if DEBUG // we don't need a million developer launch reports
				parameters.Add("user", "Debug "+s_settings.UserIdentifier);
				#else
				parameters.Add("user", s_settings.UserIdentifier);
				#endif

				string result = HttpPost("http://www.wesay.org/usage/post.php", parameters);
				return result == "OK";
			}
			catch(Exception)
			{
				Reporting.Logger.WriteMinorEvent("Http Report Failed");
				return false;
			}
		}

		/// <summary>
		/// store and retrieve values which are the same for all apps using this usage libary
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, string> GetAllApplicationValuesForThisUser()
		{
			var values = new Dictionary<string, string>();
			try
			{
				var path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				path = Path.Combine(path, "Palaso");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				path = Path.Combine(path, "usage.txt");
				if (!File.Exists(path))
				{
					//enhance: guid is our only value at this time, and we don't have a way to add more values or write them out

					//Make a single guid which connects all reports from this user, so that we can make sense of them even
					//if/when their IP address changes
					File.WriteAllText(path, @"guid==" + Guid.NewGuid().ToString());
				}
				foreach (var line in File.ReadAllLines(path))
				{
					var parts = line.Split(new string[] {"=="}, 2, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length == 2)
					{
						values.Add(parts[0].Trim(), parts[1].Trim());
					}
				}
			}
			catch (Exception error)
			{
				Debug.Fail(error.Message);// don't do anything to a non-debug user, though.
			}
			return values;
		}

		[Obsolete("Use BeginGoogleAnalytics Instead ")]
		public static void ReportLaunchesAsync()
		{
			Guard.AgainstNull(s_settings, "Client must set the settings with AppReportSettings");
			Debug.Assert(AppReportingSettings == s_settings, "CHecking to see if this is supposed to be true...");
			AppReportingSettings.Launches++;//review... should be the same as
			s_settings.PreviousLaunchDate = DateTime.UtcNow.Date;
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

			parameters.Add("launches", AppReportingSettings.Launches.ToString());


			foreach (var pair in GetAllApplicationValuesForThisUser())
			{
				parameters.Add(pair.Key,pair.Value);
			}
			#if DEBUG // we don't need a million developer launch reports
				   parameters.Add("user", "Debug");
			#endif

			//todo: notice, we don't have a way to add the user name in this one?

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
				}, null);


				return System.Text.Encoding.ASCII.GetString(response);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		///  Reports upgrades, launches, etc., and allows for later calls to notify analytics of navigation and events
		/// </summary>

		public void BeginGoogleAnalytics(string domain, string googleAnalyticsAccountCode)
		{
			_analytics = new AnalyticsEventSender(domain, googleAnalyticsAccountCode, UserGuid);

			 if (DateTime.UtcNow.Date != s_settings.PreviousLaunchDate.Date)
			{
				s_settings.PreviousLaunchDate = DateTime.UtcNow.Date;
				s_settings.Launches++;
			}

			//TODO: maybe report number of launches... depends on whether GA gives us the same data somehow
			//(i.e., how many people are return vistors, etc.)

			if (string.IsNullOrEmpty(_realPreviousVersion))
			{
				_analytics.SendNavigationNotice("firstApparentLaunchForAnyVersionOnMachine"+"/"+ErrorReport.VersionNumberString);
			}
			else if (_realPreviousVersion != ErrorReport.VersionNumberString)
			{
				_analytics.SendNavigationNotice("versionChange/version{0}-previousVersion{1}",ErrorReport.VersionNumberString,_realPreviousVersion );
			}

			if (UsageReporter.AppReportingSettings.Launches == 1)
			{
				SendNavigationNotice("firstLaunch/version{0}", ErrorReport.VersionNumberString);
			}


			//Usage.Send("Runtime", "launched", ErrorReport.VersionNumberString, UsageReporter.AppReportingSettings.Launches);
		}

		/// <summary>
		/// Send an navigation notice to Google Analytics, if BeginGoogleAnalytics was previously called
		/// Record a visit to part of the application, just as if it  were a page.
		/// Leave it up to this method to insert things like the fact that you are in DEBUG mode, or what version is being used, etc.
		/// </summary>
		/// <example>SendNavigationNotice("aboutBox"), SendNavigationNotice("dictionary/browseView")</example>
		public void SendNavigationNotice(string programArea, params object[] args)
		{
			if (!Palaso.Reporting.UsageReporter.AppReportingSettings.OkToPingBasicUsageData)
				return;
			try
			{
				if (_analytics == null)
				{
					//note for now, I'm figuring some libaries might call this, with no way to know if the host app has enabled it.
					//so we don't act like it is an error.
					Debug.WriteLine("Got Navigation notice but google analytics wasn't enabled");
					return;
				}

				//var uri = new Uri(programArea);


				var area = string.Format("SendNavigationNotice(" + programArea + ")", args);
				Debug.WriteLine(area);
				_analytics.SendNavigationNotice(programArea,args);
				_mostRecentArea = area;
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
			}
		}

		/// <summary>
		/// Send an event to Google Analytics, if BeginGoogleAnalytics was previously called
		/// </summary>
		/// <param name="programArea">DictionaryBrowse</param>
		/// <param name="action">       DeleteWord                   Error</param>
		/// <param name="optionalLabel">Some Exception Message</param>
		/// <param name="optionalInteger">some integer that makes sense for this event</param>
		/// <example>SendEvent("dictionary/browseView", "Command", "DeleteWord", "","")</example>
		/// <example>SendEvent("dictionary/browseView", "Error", "DeleteWord", "some error message we got","")</example>
		public void SendEvent(string programArea, string category, string action, string optionalLabel, int optionalInteger)
				{
					if (!Palaso.Reporting.UsageReporter.AppReportingSettings.OkToPingBasicUsageData)
						return;
 try
			{
				if (_analytics == null)
				{
					//note for now, I'm figuring some libaries might call this, with no way to know if the host app has enabled it.
					//so we don't act like it is an error.
					Debug.WriteLine("Got SendEvent notice but google analytics wasn't enabled");
					return;
				}
				Debug.WriteLine(string.Format("SendEvent(cat={0},action={1},label={2},value={3}",category,action,optionalLabel,optionalInteger));
				_analytics.SendEvent(programArea, "runtime",action,optionalLabel,optionalInteger);
			}
			catch (Exception e)
			{
#if DEBUG
				throw;
#endif
			}
				}

		/// <summary>
		/// Send an error to Google Analytics, if BeginGoogleAnalytics was previously called
		/// </summary>
		public void ReportException(bool wasFatal, string theCommandOrOtherContext, Exception error)
		{
			string message = error.Message;
			if (error.InnerException != null)
				message += " Inner: " + error.InnerException.Message;
			SendEvent(_mostRecentArea, wasFatal?"Fatal Error":"Non-Fatal Error", theCommandOrOtherContext, message, 0);
		}

	}
}
