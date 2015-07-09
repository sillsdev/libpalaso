using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace SIL.Email
{
	public class EmailProviderFactory
	{
		public static IEmailProvider PreferredEmailProvider()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				if (ThunderbirdIsDefault())
				{
					Console.WriteLine("Using Thunderbird provider");
					return new ThunderbirdEmailProvider();
				}
				if (File.Exists("/usr/bin/xdg-email"))
				{
					Console.WriteLine("Using xdg-email provider");
					return new LinuxEmailProvider();
				}
				return new MailToEmailProvider();
			}
			return new MapiEmailProvider();
		}

		public static IEmailProvider AlternateEmailProvider()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix && !ThunderbirdIsDefault() && !File.Exists("/usr/bin/xdg-email"))
			{
				return null;
			}
			return new MailToEmailProvider();
		}

		public static bool ThunderbirdIsDefault() {
			bool result = false;
			if (!result)
				result |= ThunderbirdIsDefaultOnX();
			if (!result)
				result |= ThunderbirdIsDefaultOnGnome();
			return result;
		}

		public static bool ThunderbirdIsDefaultOnGnome()
		{
			bool result = false;
			string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string preferredAppFile = String.Format(
				"{0}/.gconf/desktop/gnome/url-handlers/mailto/%gconf.xml",
				home
			);
			if (File.Exists(preferredAppFile))
			{
				XPathDocument doc = new XPathDocument(new XmlTextReader(preferredAppFile));
				XPathNavigator nav = doc.CreateNavigator();
				XPathNodeIterator it = nav.Select("/gconf/entry/stringvalue");
				if (it.MoveNext())
				{
					result = it.Current.Value.Contains("thunderbird");
				}
			}
			Console.WriteLine("Thunderbird on Gnome? Result {0}", result);
			return result;
		}

		public static bool ThunderbirdIsDefaultOnX()
		{
			bool result = false;
			string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string preferredAppFile = String.Format(
				"{0}/.config/xfce4/helpers.rc",
				home
			);
			if (File.Exists(preferredAppFile))
			{
				using (var reader = File.OpenText(preferredAppFile))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine().Trim();
						string[] keyValue = line.Split('=');
						if (keyValue.Length == 2) {
							result =  (keyValue[0] == "MailReader" && keyValue[1] == "thunderbird");
							break;
						}
					}
				}
			}
			Console.WriteLine("Thunderbird on X? Result {0}", result);
			return result;
		}

	}
}
