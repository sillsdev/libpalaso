using System;

namespace Palaso.Email
{
	public class EmailProviderFactory
	{
		public static IEmailProvider PreferredEmailProvider()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return new MailToEmailProvider();
			}
			return new MapiEmailProvider();
		}

		public static IEmailProvider AlternateEmailProvider()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return null;
			}
			return new MailToEmailProvider();
		}
	}
}