using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SIL.Reporting
{
	/// <summary>
	/// For information on what's going on here, google "goolge analytics".
	/// </summary>
	public class AnalyticsEventSender
	{
		private string _domain;
		private string _googleAccountCode;
		private readonly Guid _userId;
		private readonly DateTime _firstLaunch;
		private readonly DateTime _previousLaunch;
		private readonly int _launches;
		private readonly bool _reportAsDeveloper;
		private readonly Action<Cookie> _rememberGoogleCookie;
		private int _sequence;
		private Cookie _googleCookie;


		public AnalyticsEventSender(string domain, string googleAccountCode, Guid userId, DateTime firstLaunch, DateTime previousLaunch, int launches, bool reportAsDeveloper, Action<Cookie> RememberGoogleCookie, Cookie googleCookie)
		{
			_domain = domain;
			_googleAccountCode = googleAccountCode;
			_userId = userId;
			_firstLaunch = firstLaunch;
			_previousLaunch = previousLaunch;
			_launches = launches;
			_reportAsDeveloper = reportAsDeveloper;
			   _rememberGoogleCookie = RememberGoogleCookie;
			_googleCookie = googleCookie;


			//I don't acutally know how this is used by google... we don't have a way of giving a sequence order across
			//all users... for now we only make sure they are sequential during a given run, starting from a random number.
			_sequence = new Random().Next(1000000000);
		}

		/// <summary>
		/// http://code.google.com/apis/analytics/docs/tracking/eventTrackerGuide.html
		/// </summary>
		/// <param name="pagePath"></param>
		/// <param name="category">     Videos                  Runtime</param>
		/// <param name="action">       Pause                   Error</param>
		/// <param name="optionalLabel">Gone With The Wind      Exception Message</param>
		/// <param name="optionalInteger">32 (that is, at 32 seconds into the video)</param>
		public void SendEvent(string pagePath, string category, string action, string optionalLabel, int optionalInteger)
		{
			try
			{
				pagePath = MassagePagePath(pagePath);
				Dictionary<string, string> parameters = GetGenericParameters(pagePath);
				parameters.Add("utme", MakeEventString(category, action, optionalLabel, optionalInteger));//Extensible Parameter 	Value is encoded. Used for events and custom variables.
				parameters.Add("gaq", "1");//REVIEW: haven't found info on what this is for
				parameters.Add("utmt", "event"); //Indicates the type of request, which is one of: event, transaction, item, or custom variable. If this value is not present in the GIF request, the request is typed as page.
				SendUrlRequest(parameters);
			}
			catch (Exception)
			{
#if DEBUG//not ever worth bugging the user about
				throw;
#endif
			}
		}

		/// <summary>
		/// Record a visit to part of the application, just as if it  were a page.
		/// Leave it up to this method to insert things like the fact that you are in DEBUG mode, or what version is being used, etc.
		/// </summary>
		/// <example>SendNavigationNotice("aboutBox"), SendNavigationNotice("dictionary/browseView")</example>
		public void SendNavigationNotice(string pagePath, params object[] args)
		{
			try
			{
				pagePath = MassagePagePath(string.Format(pagePath, args));

				Dictionary<string, string> parameters = GetGenericParameters(pagePath);
				 SendUrlRequest(parameters);
			}
			catch (Exception)
			{
#if DEBUG//not ever worth bugging the user about
				throw;
#endif
			}
		}

		private string MassagePagePath(string pagePath)
		{
			if (pagePath == null || pagePath.Trim().Length == 0)
			{
				Debug.Fail("Empty path");
				pagePath = "unknown";
			}

			pagePath = pagePath.TrimStart(new char[] {'/'});
			if (!pagePath.StartsWith("application/"))
			{
				pagePath = "application/"+pagePath;
			}

			//nb: Normally this is true for DEBUG builds. We use this variable rather than a #if DEBUG because
			//sometimes a developer uses a Release build of this dll; he would still want hi activities
			//logged as a developer.

			if(_reportAsDeveloper)
			{
				pagePath = "dev/"+pagePath;
			}

			return pagePath;
		}

		private void SendUrlRequest(Dictionary<string, string> parameters)
		{
			string requestUriString = GetUrl(parameters);

			var bw = new BackgroundWorker();
			bw.DoWork += (o, args) =>
			{
//                RobustNetworkOperation.Do(proxy =>
//                                              {
				try
				{
//jh: i got tired of seeing this in the logs
//                                     Logger.WriteMinorEvent("Attempting SendUrlRequestAsync({0}",
//                                                            requestUriString);

					HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUriString);
					request.CookieContainer = new CookieContainer();
					//                                                    request.Proxy = proxy;
					if(_googleCookie!=null)
						request.CookieContainer.Add(request.RequestUri, _googleCookie);

					//warning, this uses the ui thread:
					//request.BeginGetResponse(new AsyncCallback(RespCallback), null);
					//since we're in the background anyway...
					//review but on a single core machine, might this still hang us up, or does it sleep, internally?
					HttpWebResponse response = (HttpWebResponse) request.GetResponse();

					//Review: is it the request or the response that should have the cookie?  Neither do

					CookieCollection cookieCollection = response.Cookies;
					if(cookieCollection!=null && cookieCollection.Count>0)
					{
						_rememberGoogleCookie(cookieCollection[0]);
						Debug.Assert(cookieCollection.Count==1, "(Debug mode only Did not expect multiple cookies from google analytics.");
					}
					Debug.WriteLine("Succesful SendUrlRequestAsync");// ah well
				}
				catch (WebException e)
				{
					if (e.Status == WebExceptionStatus.Timeout)
					{
						Debug.WriteLine("  TimedOut SendUrlRequestAsync");// ah well
					}
#if DEBUG
					else
					{
						throw e;
					}
#endif
				}
			};
//                                              }, null);
			bw.RunWorkerAsync();
		}

		private static void RespCallback(IAsyncResult ar)
		{

		}

		private string GetUrl(Dictionary<string, string> parameters)
		{
			var query = new StringBuilder();
			foreach (var pair in parameters)
			{
				query.AppendFormat("{0}={1}&", pair.Key, pair.Value);
			}

			return String.Format("http://www.google-analytics.com/__utm.gif?{0}",query.ToString().TrimEnd('&'));
		}

		private Dictionary<string, string> GetGenericParameters(string navigationPathStartingWithForwardSlash)
		{
			//http://www.google-analytics.com/__utm.gif?utmwv=4&utmn=769876874&utmhn=example.com&utmcs=ISO-8859-1&utmsr=1280x1024&utmsc=32-bit&utmul=en-us&utmje=1&utmfl=9.0%20%20r115&utmcn=1&utmdt=GATC012%20setting%20variables&utmhid=2059107202&utmr=0&utmp=/auto/GATC012.html?utm_source=www.gatc012.org&utm_campaign=campaign+gatc012&utm_term=keywords+gatc012&utm_content=content+gatc012&utm_medium=medium+gatc012&utmac=UA-30138-1&utmcc=__utma%3D97315849.1774621898.1207701397.1207701397.1207701397.1%3B...

			// http://www.google-analytics.com/__utm.gif?utmwv=4.6.5&utmn=488134812&utmhn=facebook.com&utmcs=UTF-8&utmsr=1024x576&utmsc=24-bit&utmul=en-gb&utmje=0&utmfl=10.0%20r42&utmdt=Facebook%20Contact%20Us&utmhid=700048481&utmr=-&utmp=%2Fwebdigi%2Fcontact&utmac=UA-3659733-5&utmcc=__utma%3D155417661.474914265.1263033522.1265456497.1265464692.6%3B%2B__utmz%3D155417661.1263033522.1.1.utmcsr%3D(direct)%7Cutmccn%3D(direct)%7Cutmcmd%3D(none)%3B

			var parameters = new Dictionary<string, string>();
			var randomGenerator = new Random();

			parameters.Add("utmwv", "4.7.2");// Analytics version
			parameters.Add("utmn", randomGenerator.Next(1000000000).ToString());// Unique ID generated for each GIF request to prevent caching of the GIF image.
			parameters.Add("utmhn", _domain ?? string.Empty);// Host Name, which is a URL-encoded string.
			parameters.Add("utmac", _googleAccountCode ?? string.Empty);
			parameters.Add("utmhid", _sequence++.ToString()); //?? Order ID, URL-encoded string.

			parameters.Add("utmcs", "UTF-8");									// Document encoding
			parameters.Add("utmsr", "-");		//enhance								// Screen Resolution
			parameters.Add("utmsc", "-");		//enhance								// Screen Resolution
			parameters.Add("utmul", "en-US");	//enhance									// user language
			parameters.Add("utmje", "0");										// java enabled or not
			parameters.Add("utmfl", "-");										// user flash version
			parameters.Add("utmdt", navigationPathStartingWithForwardSlash);			// page title
			parameters.Add("utmr", "-");											// referrer URL

			//these ones we don't care about are said to be required: http://code.google.com/apis/analytics/docs/tracking/gaTrackingTroubleshooting.html
			parameters.Add("utmp", navigationPathStartingWithForwardSlash);						// Page request of the current page. 	utmp=/testDirectory/myPage.html
			parameters.Add("utmcc", UtmcCookieString);					// cookie string (must be non-empty). Cookie values. This request parameter sends all the cookies requested from the page.

			//
			//add ip address if we have one
			//                if (!String.IsNullOrEmpty(RequestedByIpAddress))
			//                {
			//                    paramList.Add(new KeyValuePair<string, string>("utmip", RequestedByIpAddress));
			//                }
			return parameters;
		}

		private string MakeEventString(string category, string action, string optionalLabel, int optionalInteger)
		{
			//What's the 5? It's just a constant Google defines. "Google Analytics uses the value of the utme parameter to track events in the form of 5(object*action*label)(value)"
			return Uri.EscapeDataString(String.Format("5({0}*{1}*{2})({3})",
				category,
				action,
				string.IsNullOrEmpty(optionalLabel) ? string.Empty : optionalLabel,
				optionalInteger.ToString()));
		}

		private string UtmcCookieString
		{
			//http://www.morevisibility.com/analyticsblog/from-__utma-to-__utmz-google-analytics-cookies.html

			//http://www.randycullom.com/chatterbox/archives/2008/10/google_analytic.html

			//<domain hash>.<unique visitor id>.<timstamp of first visit>.<timestamp of previous (most recent) visit>.<timestamp of current visit>.<visit count>
			get
			{
				string utma = String.Format("{0}.{1}.{2}.{3}.{4}.{5}",
											DomainHash,
											GetUserId(_userId), //pseudo-unique visitor id
											DateTimeToUnixFormat(_firstLaunch), //enhance       //timstamp of first visit.
											DateTimeToUnixFormat(_previousLaunch),//enhance        //timestamp of previous (most recent) visit
											DateTimeToUnixFormat(DateTime.UtcNow),//timestamp of current visit
											_launches.ToString());      //visitcount


				string utmz = String.Format("{0}.{1}.{2}.{3}.utmcsr={4}|utmccn={5}|utmcmd={6}",
											DomainHash,
											DateTimeToUnixFormat(DateTime.UtcNow),
											"1",
											"1",
											"(direct)",//ReferralSource
											"(none)",//Campaign
											"(none)");//Medium

				string utmcc = Uri.EscapeDataString(String.Format("__utma={0};+__utmz={1};", utma, utmz));

				return (utmcc); ;
			}
		}

		private int DateTimeToUnixFormat(DateTime dateTime)
		{
			return (int)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
		}

		private int DomainHash
		{
			get { return GetDomainHash(_domain); }

		}

		internal static int GetDomainHash(string input)
		{
			// converted from the google domain hash code listed here:
			//http://www.google.com/support/forum/p/Google+Analytics/thread?tid=626b0e277aaedc3c&hl=en
			// Note JohnT: the original code is JavaScript. This means it does basic arithmetic with floats, but
			// shifts and similar ops with 32-bit ints. There may be some pathological case where doing it
			// all with 32-bit ints will give a different answer, but if so I haven't found it. It works
			// at least specifically for the known client urls (see unit tests).
			int a = 0;
			for (int index = input.Length - 1; index >= 0; index--)
			{
				char character = input[index];
				int intCharacter = character;
				a = (a << 6 & 268435455) + intCharacter + (intCharacter << 14);
				int c = a & 266338304;
				a = c != 0 ? a ^ c >> 21 : a;
			}

			return a;
		}

		/// <summary>
		/// Return a value suitable for the user ID part of a __utma cookie based on the given guid.
		/// We want something reasonably dependent on the guid and somewhat scattered over the range 1000000000,2147483647.
		/// This is not a very clever hash function but we expect guids to be pretty well distributed to begin with.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		internal static int GetUserId(Guid guid)
		{
			int start = 0;
			foreach (var b in guid.ToByteArray())
				start = start << 2 ^ b ^ start >> 10;
			while (start < 1000000000)
				start += 1000000000;
			return start;
		}

	}
}



