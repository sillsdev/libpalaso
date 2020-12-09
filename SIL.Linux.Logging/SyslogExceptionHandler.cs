using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.Linux.Logging
{
	public class SyslogExceptionHandler : ExceptionHandler
	{
		public string AppNameForLogging { get; private set; }

		private SyslogLogger _logger;

		/// <summary>
		/// Constructor. If desired, can speficy the app name to use in syslog, overriding the default.
		/// </summary>
		/// <param name="appName">App name to use in logging to syslog. If null, the default from SyslogLogger will be used.</param>
		public SyslogExceptionHandler(string appName = null)
		{
			AppNameForLogging = appName;
			_logger = new SyslogLogger(AppNameForLogging);
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
		}

		protected override bool ShowUI => false;

		protected override bool DisplayError(Exception exception)
		{
			if (exception == null)
				return true;
			List<string> errors = new List<string>();
			errors.Add(ErrorReport.GetExceptionText(exception));
			errors.Add("Standard ErrorReport properties for above exception:");
			var errorProperties = new SortedDictionary<string, string>(ErrorReport.GetStandardProperties());
			errors.AddRange(errorProperties.Select(property => String.Format("{0}: {1}", property.Key, property.Value)));
			/* Commented out: Adding platform properties is unnecessary since ErrorReport's OSVersion property will tell us more anyway
			errors.Add("Platform properties for above exception:");
			foreach (PropertyInfo property in typeof(Platform).GetProperties())
			{
				errors.Add(String.Format("{0}: {1}", property.Name, property.GetValue(typeof(Platform), null)));
			}
			*/
			_logger.LogMany(SyslogPriority.Error, errors);
			return true;
		}
	}
}