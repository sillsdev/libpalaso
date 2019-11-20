using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace SIL.Reporting
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
		private Exception _mostRecentException;

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
			settings.PreviousLaunchDate = DateTime.Now.Date;
			s_singleton._mostRecentArea = "Initializing"; // Seems more useful to put in something in case an error occurs before app gets this set.
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
				if (s_singleton != null)
				{
					lock (s_singleton)
					{
						if (!String.IsNullOrEmpty(s_singleton._appNameToUseInDialogs))
						{
							return s_singleton._appNameToUseInDialogs;
						}
					}
				}
				return EntryAssembly.ProductName;
			}
			set
			{
				lock (s_singleton)
					s_singleton._appNameToUseInDialogs = value;
			}
		}

		public static string AppNameToUseInReporting
		{
			get
			{
				if (s_singleton != null)
				{
					lock (s_singleton)
					{
						if (!String.IsNullOrEmpty(s_singleton._appNameToUseInReporting))
						{
							return s_singleton._appNameToUseInReporting;
						}
					}
				}
				return EntryAssembly.ProductName;
			}
			set
			{
				lock (s_singleton)
					s_singleton._appNameToUseInReporting = value;
			}
		}

		/// <summary>
		/// used for testing purposes
		/// </summary>
		public static void ResetSettingsForTests()
		{
			// RegistryAccess.SetStringRegistryValue("launches", "0");
			if (s_singleton == null)
				return;

			lock (s_singleton)
			{
				if (s_singleton._settings == null)
					s_singleton._settings = new ReportingSettings();

				s_singleton._settings.Launches = 0;
				s_singleton.MakeLaunchDateSafe();
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

		/// <summary>
		///  Reports upgrades, launches, etc., and allows for later calls to notify analytics of navigation and events
		/// </summary>
		private void BeginGoogleAnalytics(string domain, string googleAnalyticsAccountCode, bool reportAsDeveloper)
		{
				var osLabel = ErrorReport.GetOperatingSystemLabel();

				_analytics = new AnalyticsEventSender(domain, googleAnalyticsAccountCode, UserGuid, _settings.FirstLaunchDate, _settings.PreviousLaunchDate, _settings.Launches, reportAsDeveloper, SaveCookie, null/*COOKIE TODO*/);

				if (DateTime.UtcNow.Date != _settings.PreviousLaunchDate.Date)
				{
					SendNavigationNotice("{0}/launch/version{1}", osLabel, ErrorReport.VersionNumberString);
				}

				//TODO: maybe report number of launches... depends on whether GA gives us the same data somehow
				//(i.e., how many people are return vistors, etc.)

				if (string.IsNullOrEmpty(_realPreviousVersion))
				{
					SendNavigationNotice("{0}/firstApparentLaunchForAnyVersionOnMachine" + "/" + ErrorReport.VersionNumberString, osLabel);
				}
				else if (_realPreviousVersion != ErrorReport.VersionNumberString)
				{
					SendNavigationNotice("{0}/versionChange/version{1}-previousVersion{2}", osLabel, ErrorReport.VersionNumberString, _realPreviousVersion);
				}
		}

		private void SaveCookie(Cookie cookie)
		{
			//ErrorReportSettings.Default.AnalyticsCookie = cookie; /*TODO: how to serialize the cookie?*/
			//ErrorReportSettings.Default.Save();
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

			lock (s_singleton)
			{
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
				catch (Exception)
				{
#if DEBUG
					throw;
#endif
				}
			}
		}

		/// <summary>
		/// Send an event to Google Analytics, if BeginGoogleAnalytics was previously called
		/// </summary>
		/// <param name="programArea">DictionaryBrowse</param>
		/// <param name="category"></param>
		/// <param name="action">       DeleteWord                   Error</param>
		/// <param name="optionalLabel">Some Exception Message</param>
		/// <param name="optionalInteger">some integer that makes sense for this event</param>
		/// <example>SendEvent("dictionary/browseView", "Command", "DeleteWord", "","")</example>
		/// <example>SendEvent("dictionary/browseView", "Error", "DeleteWord", "some error message we got","")</example>
		public static void SendEvent(string programArea, string category, string action, string optionalLabel, int optionalInteger)
		{
			if (s_singleton == null)
				return;

			lock (s_singleton)
			{
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
					Debug.WriteLine(string.Format("SendEvent(cat={0},action={1},label={2},value={3}", category, action, optionalLabel, optionalInteger));
					s_singleton._analytics.SendEvent(programArea, category, action, optionalLabel, optionalInteger);
				}
				catch (Exception)
				{
#if DEBUG
					throw;
#endif
				}
			}
		}

		/// <summary>
		/// Send an error to Google Analytics, if BeginGoogleAnalytics was previously called
		/// </summary>
		public static void ReportException(bool wasFatal, string theCommandOrOtherContext, Exception error, string messageUserSaw)
		{
			if (s_singleton == null)
				return;

			lock (s_singleton)
			{
				if (error != null && s_singleton._mostRecentException == error)
					return; //it's hard to avoid getting here twice with all the various paths

				s_singleton._mostRecentException = error;

				var sb = new StringBuilder();
				if (!string.IsNullOrEmpty(messageUserSaw))
					sb.Append(messageUserSaw + "|");
				if (error != null)
					sb.Append(error.Message + "|");
				if (s_singleton._mostRecentArea != null)
					sb.Append(s_singleton._mostRecentArea + "|");
				if (!string.IsNullOrEmpty(theCommandOrOtherContext))
					sb.Append(theCommandOrOtherContext + "|");
				if (wasFatal)
					sb.Append(" fatal |");
				if (error != null)
				{
					if (error.InnerException != null)
						sb.Append("Inner: " + error.InnerException.Message + "|");
					sb.Append(error.StackTrace);
				}
				// Maximum URI length is about 2000 (probably 2083 to be exact), so truncate this info if ncessary.
				// A lot of characters (such as spaces) are going to be replaced with % codes, and there is a pretty hefty
				// wad of additional stuff that goes into the URL besides this stuff, so cap it at 1000 and hope for the best.
				if (sb.Length > 1000)
					sb.Length = 1000;

				SendEvent(s_singleton._mostRecentArea, "error", sb.ToString(), ErrorReport.VersionNumberString, 0);
			}
		}

		public static void ReportException(Exception error)
		{
			ReportException(false, null, error, null);
		}

		public static void ReportExceptionString(string messageUserSaw)
		{
			ReportException(false, null, null, messageUserSaw);
		}
	}
}
