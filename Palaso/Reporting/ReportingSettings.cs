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
			PreviousLaunchDate = default(DateTime);
			OkToPingBasicUsageData = true;
			HaveShowRegistrationDialog = false;
			PreviousVersion = string.Empty;
		}

		public int Launches { get; set;}

		public string UserIdentifier { get; set; }

		public DateTime PreviousLaunchDate { get; set; }

		public bool OkToPingBasicUsageData { get; set; }

		public bool HaveShowRegistrationDialog { get; set; }

		/// <summary>
		/// help notice that someone upgraded
		/// </summary>
		public string PreviousVersion { get; set; }


	}
}
