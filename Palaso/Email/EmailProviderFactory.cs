using System;
using System.Xml;
using System.Xml.XPath;

namespace Palaso.Email
{
	public class EmailProviderFactory
	{
		public static IEmailProvider PreferredEmailProvider()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				if (ThunderbirdIsDefault())
				{
					return new ThunderbirdEmailProvider();
				}
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

		public static bool ThunderbirdIsDefault()
		{
			bool result = false;
			string home = Environment.GetEnvironmentVariable("HOME");
			string preferredAppFile = String.Format(
				"{0}/.gconf/desktop/gnome/url-handlers/mailto/%gconf.xml",
				home
			);
			XPathDocument doc = new XPathDocument(new XmlTextReader(preferredAppFile));
			XPathNavigator nav = doc.CreateNavigator();
			XPathNodeIterator it = nav.Select("/gconf/entry/stringvalue");
			if (it.MoveNext())
			{
				result = it.Current.Value.Contains("thunderbird");
			}
			Console.WriteLine("Result {0}", result);
			return result;
		}
	}
}