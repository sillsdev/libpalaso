using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Palaso.Reporting
{

	[Serializable]
	public class ReportingSettings
	{

		public ReportingSettings()
		{
			// TODO set defaults
		}

		public int Launches
		{
			get;
			set;
		}

		public string UserIdentifier { get; set; }

		public DateTime LastLaunchDate { get; set; }

		public bool OkToPingBasicUsageData { get; set; }

		public bool HaveShowRegistrationDialog { get; set; }


	}
}
