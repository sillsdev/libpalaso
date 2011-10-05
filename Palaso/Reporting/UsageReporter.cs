using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public class UsageReporter
	{
		private  string _appNameToUseInDialogs;
		private  string _appNameToUseInReporting;
		private  ReportingSettings _settings;
		private  AnalyticsEventSender _analytics;
		private string _realPreviousVersion;
		private string _mostRecentArea;

		private static UsageReporter s_singleton;

		[Obsolete("Better to use the version which explicitly sets the reportAsDeveloper flag")]
		public static void Init(ReportingSettings settings, string domain, string googleAnalyticsAccountCode)
		{
#if DEBUG
			Init(settings,domain,googleAnalyticsAccountCode, true);
#else
			Init(settings,domain,googleAnalyticsAccountCode, false);
#endif
		}

		/// <summary>
		///
		/// </summary>
		/// <example>
		/// UsageReporter.Init(Settings.Default.Reporting, "myproduct.org", "UA-11111111-2",
		///#if DEBUG
		///                true
		///#else
		///                false
		///#endif
		///                );
		/// </example>
		/// <param name="settings"></param>
		/// <param name="domain"></param>
		/// <param name="googleAnalyticsAccountCode"></param>
		/// <param name="reportAsDeveloper">Normally this is true for DEBUG builds. It is separated out here because sometimes a developer
		/// uses a Release build of Palaso.dll, but would still want his/her activities logged as a developer.</param>
		public static void Init(ReportingSettings settings, string domain, string googleAnalyticsAccountCode, bool reportAsDeveloper)
		{
			s_singleton = new UsageReporter();
			s_singleton._settings = settings;
			s_singleton._realPreviousVersion = settings.PreviousVersion;
			s_singleton._settings.Launches++;
			s_singleton.BeginGoogleAnalytics(domain, googleAnalyticsAccountCode, reportAsDeveloper);
			settings.PreviousVersion = ErrorReport.VersionNumberString;
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

/*        private static void GetUserIdentifierIfNeeded( )
		{
			Guard.AgainstNull(_settings, "Client must set the settings with AppReportSettings");

			//nb, this tries to share the id between applications that might want it,
			//so the user doesn't have to be asked again.

			string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "SIL");
			Directory.CreateDirectory(dir);
			string path = Path.Combine(dir, "UserIdentifier.txt");
		   // ReportingSetting.Default.Identifier = "";
			if (!_settings.HaveShowRegistrationDialog)
			{
					_settings.HaveShowRegistrationDialog = true;
					UserRegistrationDialog dlg = new UserRegistrationDialog();
					if (File.Exists(path))
					{
						_settings.UserIdentifier = File.ReadAllText(path);
					}
					dlg.ShowDialog();
					_settings.UserIdentifier = dlg.EmailAddress;
					_settings.OkToPingBasicUsageData= dlg.OkToCollectBasicStats;

					//NB: the current system requires that the caller do the saving
					//of the other settings.

					File.WriteAllText(path, _settings.UserIdentifier);
			}

		}
		*/

		/// <summary>
		/// cover an apparent bug in the generated code when you do a get but the datetime is null
		/// </summary>
		private void MakeLaunchDateSafe( )
		{
			try
			{
				DateTime dummy = _settings.PreviousLaunchDate;
			}
			catch
			{
				_settings.PreviousLaunchDate = default(DateTime);
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

/*		public static ReportingSettings AppReportingSettings
		{
			get
			{
				return _settings;
			}
			set
			{
				_settings = value;
			}
		}
	*/

		//TODO: I think this would fit better in ErrorReport
		public static string AppNameToUseInDialogs
		{
			get
			{

				if (s_singleton != null && !String.IsNullOrEmpty(s_singleton._appNameToUseInReporting))
				{
				 return s_singleton._appNameToUseInDialogs;

				}
				return Application.ProductName;
			}
			set
			{
				s_singleton._appNameToUseInDialogs = value;
			}
		}

		public static string AppNameToUseInReporting
		{
			get
			{
				if (s_singleton != null && !String.IsNullOrEmpty(s_singleton._appNameToUseInReporting))
				{
					 return s_singleton._appNameToUseInReporting;
				}
				return Application.ProductName;
			}
			set
			{
				s_singleton._appNameToUseInReporting = value;
			}
		}

		/// <summary>
		/// used for testing purposes
		/// </summary>
		public static void ResetSettingsForTests()
		{
		   // RegistryAccess.SetStringRegistryValue("launches", "0");
			if (s_singleton != null && s_singleton._settings == null)
			{
				s_singleton._settings = new ReportingSettings();
			}

			s_singleton._settings.Launches = 0;
			s_singleton.MakeLaunchDateSafe();
		}


		/*     /// <summary>
			 /// if you call this every time the application starts, it will send reports on those intervals
			 /// (e.g. {1, 10}) that are listed in the intervals parameter.  It will get version number and name out of the application.
			 /// </summary>
			 [Obsolete("Use BeginGoogleAnalytics Instead ")]
			 public static void DoTrivialUsageReport(string emailAddress, string topMessage, int[] intervals)
			 {
				 Guard.AgainstNull(_settings, "Client must set the settings with AppReportSettings");

				 MakeLaunchDateSafe();

				 //avoid asking the user more than once on the special reporting days
				 if (DateTime.UtcNow.Date != _settings.PreviousLaunchDate.Date)
				 {
					 foreach (int launch in intervals)
					 {
						 if (launch == _settings.Launches)
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
										   _settings.Launches);
						 d.EmailMessage.Body =
							 string.Format("app={0} version={1} launches={2}",
										   UsageReporter.AppNameToUseInReporting,
										   version,
										   _settings.Launches);
						 d.ShowDialog();
					 }
				 }
			 }



		public static bool AttemptHttpReport()
		{
			Guard.AgainstNull(_settings, "Client must set the settings with AppReportSettings");

			try
			{
				if(!_settings.OkToPingBasicUsageData)
					return false;

				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("app", UsageReporter.AppNameToUseInReporting);
				parameters.Add("version", ErrorReport.VersionNumberString);
				parameters.Add("launches", _settings.Launches.ToString());

				#if DEBUG // we don't need a million developer launch reports
				parameters.Add("user", "Debug "+_settings.UserIdentifier);
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
		 */

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
/*
		[Obsolete("Use BeginGoogleAnalytics Instead ")]
		public static void ReportLaunchesAsync()
		{
			Guard.AgainstNull(_settings, "Client must set the settings with AppReportSettings");
			Debug.Assert(AppReportingSettings == _settings, "CHecking to see if this is supposed to be true...");
			AppReportingSettings.Launches++;//review... should be the same as
			_settings.PreviousLaunchDate = DateTime.UtcNow.Date;
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
		*/

		/// <summary>
		///  Reports upgrades, launches, etc., and allows for later calls to notify analytics of navigation and events
		/// </summary>

		private void BeginGoogleAnalytics(string domain, string googleAnalyticsAccountCode, bool reportAsDeveloper)
		{
			if (DateTime.UtcNow.Date != _settings.PreviousLaunchDate.Date)
			{
				_settings.Launches++;
			}

			_analytics = new AnalyticsEventSender(domain, googleAnalyticsAccountCode, UserGuid, _settings.FirstLaunchDate, _settings.PreviousLaunchDate, _settings.Launches, reportAsDeveloper, SaveCookie, null/*COOKIE TODO*/);

			 if (DateTime.UtcNow.Date != _settings.PreviousLaunchDate.Date)
			{
				_settings.Launches++;
				SendNavigationNotice("launch/version{0}", ErrorReport.VersionNumberString);
			}

			//TODO: maybe report number of launches... depends on whether GA gives us the same data somehow
			//(i.e., how many people are return vistors, etc.)

			if (string.IsNullOrEmpty(_realPreviousVersion))
			{
				SendNavigationNotice("firstApparentLaunchForAnyVersionOnMachine"+"/"+ErrorReport.VersionNumberString);
			}
			else if (_realPreviousVersion != ErrorReport.VersionNumberString)
			{
				SendNavigationNotice("versionChange/version{0}-previousVersion{1}",ErrorReport.VersionNumberString,_realPreviousVersion );
			}

			if (s_singleton._settings.Launches == 1)
			{
				SendNavigationNotice("firstLaunch/version{0}", ErrorReport.VersionNumberString);
			}


			//Usage.Send("Runtime", "launched", ErrorReport.VersionNumberString, UsageReporter.AppReportingSettings.Launches);
		}

		private void SaveCookie(Cookie cookie)
		{
			//ErrorReportSettings.Default.AnalyticsCookie = cookie; /*TODO: how to serialize the cookie?*/
			ErrorReportSettings.Default.Save();
		}

		/// <summary>
		/// Send an navigation notice to Google Analytics, if BeginGoogleAnalytics was previously called
		/// Record a visit to part of the application, just as if it  were a page.
		/// Leave it up to this method to insert things like the fact that you are in DEBUG mode, or what version is being used, etc.
		/// </summary>
		/// <example>SendNavigationNotice("aboutBox"), SendNavigationNotice("dictionary/browseView")</example>
		public static void SendNavigationNotice(string programArea, params object[] args)
		{
			if (s_singleton == null)
				return;

			if (!s_singleton._settings.OkToPingBasicUsageData)
				return;
			try
			{
				if (s_singleton._analytics == null)
				{
					//note for now, I'm figuring some libaries might call this, with no way to know if the host app has enabled it.
					//so we don't act like it is an error.
					Debug.WriteLine("Got Navigation notice but google analytics wasn't enabled");
					return;
				}

				//var uri = new Uri(programArea);


				var area = string.Format(programArea, args);
				Debug.WriteLine("SendNavigationNotice(" + area + ")");
				s_singleton._analytics.SendNavigationNotice(area);
				s_singleton._mostRecentArea = area;
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
		public static void SendEvent(string programArea, string category, string action, string optionalLabel, int optionalInteger)
				{
					if (s_singleton == null)
						return;

			if (!s_singleton._settings.OkToPingBasicUsageData)
						return;
 try
			{
				if (s_singleton._analytics == null)
				{
					//note for now, I'm figuring some libaries might call this, with no way to know if the host app has enabled it.
					//so we don't act like it is an error.
					Debug.WriteLine("Got SendEvent notice but google analytics wasn't enabled");
					return;
				}
				Debug.WriteLine(string.Format("SendEvent(cat={0},action={1},label={2},value={3}",category,action,optionalLabel,optionalInteger));
				s_singleton._analytics.SendEvent(programArea, category, action, optionalLabel, optionalInteger);
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
		public static void ReportException(bool wasFatal, string theCommandOrOtherContext, Exception error)
		{
			if (s_singleton == null)
				return;

			string message = error.Message;
			if (error.InnerException != null)
				message += " Inner: " + error.InnerException.Message;
			SendEvent(s_singleton._mostRecentArea, "error", message, ErrorReport.VersionNumberString + (wasFatal ? "/Fatal Error/" : "/Non-Fatal Error/")+theCommandOrOtherContext, 0);
		}

		public static void ReportException(Exception error)
		{
			if (s_singleton == null)
				return;

			string message = error.Message;
			if (error.InnerException != null)
				message += " Inner: " + error.InnerException.Message;
			SendEvent(s_singleton._mostRecentArea, "error", message,  ErrorReport.VersionNumberString , 0);
		}

		public static void ReportExceptionString(string errorMessage)
		{
			if (s_singleton == null)
				return;

			SendEvent(s_singleton._mostRecentArea, "error", errorMessage, ErrorReport.VersionNumberString, 0);
		}
	}
}
