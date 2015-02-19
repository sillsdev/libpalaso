using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Net;

namespace SIL.WritingSystems
{
	public static class Sldr
	{
		public static XNamespace Sil = "urn://www.sil.org/ldml/0.1";
		private const string SldrRepository = "https://ldml.api.sil.org/";
		private const string DefaultExtension = "ldml";

		// Default list of elements to request from the SLDR.
		// Identity is always published, so we don't need it on the list.
		private static readonly IEnumerable<string> DefaultTopElements = new List<string>
		{
			"characters",
			"delimiters",
			"layout",
			"numbers",
			"collations",
			"special"
		};
 
		// If the user wants to request a new UID, you use "uid=unknown" and that will create a new random identifier
		public const string UnknownUserId = "unknown";

		/// <summary>
		/// API request to return an LDML file and save it
		/// </summary>
		/// <param name="filename">Full filename to save the requested LDML file</param>
		/// <param name="bcp47Tag">Current BCP47 tag which is a concatenation of the Language, Script, Region and Variant properties</param>
		/// <param name="topLevelElements">List of top level element names to request. SLDR will always publish identity, so it doesn't need to be requested.
		/// If null, the default list of {"characters", "delimiters", "layout", "numbers", "collations", "special"} will be requested.</param>
		/// <param name="flatten">Currently not supported.  Indicates whether or not you want to include all the data 
		/// inherited from a more general file.  SLDR currently defaults to true (1)</param>
		public static void GetLdmlFile(string filename, string bcp47Tag, IEnumerable<string> topLevelElements, Boolean flatten = true)
		{
			if (String.IsNullOrEmpty(filename))
			{
				throw new ArgumentException("filename");
			}
			if (String.IsNullOrEmpty(bcp47Tag))
			{
				throw new ArgumentException("bcp47Tag");
			}
			if (topLevelElements == null)
			{
				throw new ArgumentException("topLevelElements");
			}

			// Random 8-character identifier.  This marks various bits of information as alternatives that have been suggested by that specific user.
			string userId = UnknownUserId;

			// If the LDML file already exists, attempt to extract userId
			if (File.Exists(filename))
			{
				var fi = new FileInfo(filename);
				if (fi.Length > 0)
				{
					XElement element = XElement.Load(filename);
					XElement identityElem = element.Descendants(Sil + "identity").First();
					if (identityElem != null)
						userId = (string) identityElem.Attribute("uid") ?? UnknownUserId;
				}
			}

			// Concatenate requested top level elements
			string requestedElements = string.Join("&inc[]=", topLevelElements);

			// Concatenate url string
			string url = string.Format("{0}{1}?uid={2}&ext={3}&inc[]={4}&flatten={5}", SldrRepository, bcp47Tag, userId, DefaultExtension, requestedElements, Convert.ToInt32(flatten));
			string tempFilename = filename + ".tmp";
			try
			{
				// Download the LDML file to a temp file in case the transfer gets interrupted
				var webClient = new WebClient();
				webClient.DownloadFile(Uri.EscapeUriString(url), tempFilename);

				// Move the completed temp file to filename
				if (File.Exists(filename))
					File.Delete(filename);
				File.Move(tempFilename, filename);

				// the SLDR currently has a bug where the SIL namespace is not properly defined
				// TODO: Add it manually to the special blocks
				// TODO: remove this when the SLDR fixes this bug
			}
			catch (Exception)
			{
				// Cleanup temp file
				if (File.Exists(tempFilename))
					File.Delete(tempFilename);
				throw;
			}
		}

		/// <summary>
		/// API request to return an LDML file and save it
		/// </summary>
		/// <param name="filename">Full filename to save the requested LDML file</param>
		/// <param name="bcp47Tag">Current BCP47 tag which is a concatenation of the Language, Script, Region and Variant properties</param>
		public static void GetLdmlFile(string filename, string bcp47Tag)
		{
			GetLdmlFile(filename, bcp47Tag, DefaultTopElements);
		}
	}
}
