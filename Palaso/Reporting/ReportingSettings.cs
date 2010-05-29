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
			UserIdentifier = string.Empty;
			LastLaunchDate = default(DateTime);
			OkToPingBasicUsageData = true;
			HaveShowRegistrationDialog = false;
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
