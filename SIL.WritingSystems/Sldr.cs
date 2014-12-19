using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Net;
using System.Web;

namespace SIL.WritingSystems
{
	public static class Sldr
	{
		public static XNamespace Sil = "urn://www.sil.org/ldml/0.1";
		private const string SldrRepository = "https://ldml.api.sil.org/";

		// If the user wants to request a new UID, you use "uid=unknown" and that will create a new random identifier
		public const string UnknownUserId = "unknown";

		/// <summary>
		/// API request to return an LDML file and save it
		/// </summary>
		/// <param name="filename">Full filename to save the requested LDML file</param>
		/// <param name="bcp47Tag">Current BCP47 tag which is a concatenation of the Language, Script, Region and Variant properties</param>
		/// <param name="flatten">Currently not supported.  Indicates whether or not you want to include all the data 
		/// inherited from a more general file.  SLDR currently defaults to true (1)</param>
		public static void GetLdmlFile(string filename, string bcp47Tag, Boolean flatten = true)
		{
			if (String.IsNullOrEmpty(filename))
			{
				throw new ArgumentException("filename");
			}
			if (String.IsNullOrEmpty(bcp47Tag))
			{
				throw new ArgumentException("bcp47Tag");
			}

			// Random 8-character identifier.  This marks various bits of information as alternatives that have been suggested by that specific user.
			string userId = UnknownUserId;

			// If the LDML file already exists, attempt to extract userId
			if (File.Exists(filename))
			{
				XElement element = XElement.Load(filename);
				XElement identityElem = element.Descendants(Sil + "identity").First(); 
				if (identityElem != null)
				{
					userId = (string) identityElem.Attribute("uid") ?? UnknownUserId;
				}
			}

			// Concatenate url string
			string url = string.Format("{0}{1}?uid={2}&flatten={3}", SldrRepository, HttpUtility.UrlEncode(bcp47Tag) , HttpUtility.UrlEncode(userId), Convert.ToInt32(flatten));
			string tempFilename = filename + ".tmp";
			try
			{
				// Download the LDML file to a temp file in case the transfer gets interrupted
				WebClient webClient = new WebClient();
				webClient.DownloadFile(url, tempFilename);

				// Move the completed temp file to filename
				if (File.Exists(filename))
					File.Delete(filename);
				File.Move(tempFilename, filename);
			}
			catch (Exception)
			{
				// Cleanup temp file
				if (File.Exists(tempFilename))
					File.Delete(tempFilename);
				throw;
			}
		}

	}
}
