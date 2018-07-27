using System;

namespace SIL.Reporting
{

	[Serializable]
	public class ReportingSettings
	{

		public ReportingSettings()
		{
			UserIdentifier = string.Empty;
			FirstLaunchDate = PreviousLaunchDate = DateTime.UtcNow;
			OkToPingBasicUsageData = true;
			UserOkToAutoUpdate = true;
			HaveShowRegistrationDialog = false;
			PreviousVersion = string.Empty;
		}

		public int Launches { get; set;}

		public string UserIdentifier { get; set; }

		public DateTime FirstLaunchDate { get; set; }
		public DateTime PreviousLaunchDate { get; set; }
		public bool OkToPingBasicUsageData { get; set; }
		public bool UserOkToAutoUpdate { get; set; }
		public bool HaveShowRegistrationDialog { get; set; }

		/// <summary>
		/// help notice that someone upgraded
		/// </summary>
		public string PreviousVersion { get; set; }


	}
}
