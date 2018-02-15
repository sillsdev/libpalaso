// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// LanguageData is a separate program for gathering standard language information together
// It should be run when you want to check for new data files or create the LanguageDataIndex.txt
// See https://github.com/sillsdev/libpalaso/wiki/LanguageData for more details
namespace LanguageData
{
	/// <summary>
	/// This is the part of the LanguageData app that gets the source data and checks if it has changed
	/// it writes a file LastModified.txt which says when each of the data files were last modified on their servers
	/// The data files are:
	///    ianaSubtagRegistry.txt - http://www.iana.org/assignments/language-subtag-registry/language-subtag-registry
	///        iana subtag registry which is the primary resource for language tags
	///    LanguageCodes.txt - https://www.ethnologue.com/codes/LanguageCodes.tab - language codes and countries from the ethnologue
	///    LanguageIndex.txt - https://www.ethnologue.com/codes/LanguageIndex.tab - main ethnologue data
	///    http://www-01.sil.org/iso639-3/iso-639-3.tab is used to create TwoToThreeCodes.txt
	/// The data files do not change often, maybe 2 or 3 times per year each
	///
	/// If you add a new data source then you will need to add it to the comments, GetNewSources, GetOldSources,
	///    CheckSourcesAreDifferent, WriteNewFiles and GetFileStrings
	/// </summary>
	public class GetAndCheckSources
	{
		private string _oldtwotothree;
		private string _oldlanguageindex;
		private string _oldlanguagecodes;
		private string _oldianasubtags;

		private string _newtwotothree;
		private string _newlanguageindex;
		private string _newlanguagecodes;
		private string _newianasubtags;

		// solution to teamcity problems with certificates
		// found in http://stackoverflow.com/questions/4926676/mono-webrequest-fails-with-https
		private bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isOk = true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i=0; i<chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
					{
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build ((X509Certificate2)certificate);
						if (!chainIsValid)
						{
							isOk = false;
						}
					}
				}
			}
			return isOk;
		}

		/// <summary>
		/// Get the current versions of the files from their websites
		/// </summary>
		/// <returns>true if it succeeds in downloading the current versions of the files</returns>
		public bool GetNewSources()
		{
			try
			{
				// Create web client.
				ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
				WebClient client = CreateWebClient();

				// Download strings.
				string newiso693 = client.DownloadString("http://www-01.sil.org/iso639-3/iso-639-3.tab");
				string lastmod_iso693 = client.ResponseHeaders["Last-Modified"];

				client.Encoding = Encoding.UTF8; // ethnologue site currently doesn't specify encoding 2017-01-24
				_newlanguageindex = client.DownloadString("https://www.ethnologue.com/codes/LanguageIndex.tab");
				_newlanguageindex = _newlanguageindex.Replace("\r\n", "\n");
				string lastmod_languageindex = client.ResponseHeaders["Last-Modified"];

				_newlanguagecodes = client.DownloadString("https://www.ethnologue.com/codes/LanguageCodes.tab");
				_newlanguagecodes = _newlanguagecodes.Replace("\r\n", "\n");
				string lastmod_languagecodes = client.ResponseHeaders["Last-Modified"];

				_newianasubtags = client.DownloadString("http://www.iana.org/assignments/language-subtag-registry/language-subtag-registry");
				_newianasubtags = _newianasubtags.Replace("\r\n", "\n");
				string lastmod_ianasubtag = client.ResponseHeaders["Last-Modified"];

				Console.WriteLine("IANA subtags last modified: " + lastmod_ianasubtag);
				Console.WriteLine("Ethnologue index last modified: " + lastmod_languageindex);
				Console.WriteLine("Ethnologue codes list last modified: " + lastmod_languagecodes);
				Console.WriteLine("ISO693-3 table last modified: " + lastmod_iso693);

				using (StreamWriter file = new StreamWriter(@"LastModified.txt"))
				{
					file.WriteLine("IANA subtags last modified: " + lastmod_ianasubtag);
					file.WriteLine("Ethnologue index last modified: " + lastmod_languageindex);
					file.WriteLine("Ethnologue codes list last modified: " + lastmod_languagecodes);
					file.WriteLine("ISO693-3 table last modified: " + lastmod_iso693);
				}

				_newtwotothree = GenerateTwoToThreeCodes(newiso693);
				return true;
			}
			catch (WebException)
			{
				return false;
			}
		}

		protected virtual WebClient CreateWebClient()
		{
			return new WebClient();
		}

		/// <summary>
		/// Read the current data files: TwoToThreeCodes.txt, LanguageIndex.txt, LanguageCodes.txt, ianaSubtagRegistry.txt
		/// </summary>
		/// <param name="input_dir">Directory to find the current data files</param>
		public void GetOldSources(string input_dir)
		{
			_oldtwotothree = File.ReadAllText(Path.Combine (input_dir, @"TwoToThreeCodes.txt"));
			_oldtwotothree = _oldtwotothree.Replace("\r\n", "\n");
			_oldlanguageindex = File.ReadAllText(Path.Combine (input_dir, @"LanguageIndex.txt"));
			_oldlanguageindex = _oldlanguageindex.Replace("\r\n", "\n");
			_oldlanguagecodes = File.ReadAllText(Path.Combine(input_dir, @"LanguageCodes.txt"));
			_oldlanguagecodes = _oldlanguagecodes.Replace("\r\n", "\n");
			_oldianasubtags = File.ReadAllText(Path.Combine (input_dir, @"ianaSubtagRegistry.txt"));
			_oldianasubtags = _oldianasubtags.Replace("\r\n", "\n");
		}

		/// <summary>
		/// Check if old and new sources are different
		/// </summary>
		/// <returns>true if any downloaded files are different from the current ones</returns>
		public bool CheckSourcesAreDifferent()
		{
			// return true if any are different, false if all the same
			bool retval = false;
			if (AreFilesDifferent(_oldianasubtags, _newianasubtags))
			{
				Console.WriteLine("There is a new IANA Subtags registry available");
				retval = true;
			}
			else
			{
				Console.WriteLine("The IANA Subtags registry has not changed");
			}

			if (AreFilesDifferent(_oldlanguageindex, _newlanguageindex))
			{
				retval = true;
				Console.WriteLine("There is a new Ethnologue Language Index available");
			}
			else
			{
				Console.WriteLine("The Ethnologue Language Index has not changed");
			}

			if (AreFilesDifferent(_oldlanguagecodes, _newlanguagecodes))
			{
				retval = true;
				Console.WriteLine("There is a new Ethnologue Language Codes list available");
			}
			else
			{
				Console.WriteLine("The Ethnologue Language Language Codes list has not changed");
			}

			if (AreFilesDifferent(_oldtwotothree, _newtwotothree))
			{
				retval = true;
				Console.WriteLine("There are new 2 to 3 letter code mappings available");
			}
			else
			{
				Console.WriteLine("The 2 to 3 letter code mapping has not changed");
			}


			return retval;
		}

		// this does not check that it is a safe place to put the files
		/// <summary>
		/// Save the downloaded files in a specified directory
		/// </summary>
		/// <param name="output_directory">Directory to save the files</param>
		public void WriteNewFiles(string output_directory)
		{
			if (!Uri.IsWellFormedUriString(output_directory, UriKind.RelativeOrAbsolute) || !Directory.Exists(output_directory))
			{
				throw new DirectoryNotFoundException();
			}
			string filename = Path.Combine(output_directory, "LanguageIndex.txt");
			File.WriteAllText(filename, _newlanguageindex);
			filename = Path.Combine(output_directory, "LanguageCodes.txt");
			File.WriteAllText(filename, _newlanguagecodes);
			filename = Path.Combine(output_directory, "ianaSubtagRegistry.txt");
			File.WriteAllText(filename, _newianasubtags);
			filename = Path.Combine(output_directory, "TwoToThreeCodes.txt");
			File.WriteAllText(filename, _newtwotothree);
		}


		/// <summary>
		/// Get a dictionary of files names and the string contents of the file
		/// This is to be passed to LanguageDataIndex to process
		/// </summary>
		/// <param name="newfiles">true means use new files, false means current files</param>
		/// <returns>Dictionary of files names and the string contents of the file</returns>
		public IDictionary<string,string> GetFileStrings(bool newfiles)
		{
			var filestrings = new Dictionary<string,string>();
			if (newfiles) {
				filestrings.Add("TwoToThreeCodes.txt", _newtwotothree);
				filestrings.Add("LanguageIndex.txt", _newlanguageindex);
				filestrings.Add("LanguageCodes.txt", _newlanguagecodes);
				filestrings.Add("ianaSubtagRegistry.txt", _newianasubtags);
			} else {
				filestrings.Add("TwoToThreeCodes.txt", _oldtwotothree);
				filestrings.Add("LanguageIndex.txt", _oldlanguageindex);
				filestrings.Add("LanguageCodes.txt", _oldlanguagecodes);
				filestrings.Add("ianaSubtagRegistry.txt", _oldianasubtags);
			}
			return filestrings;
		}

		private string GenerateTwoToThreeCodes(string iso693)
		{
			// JohnT: can't find anywhere else to document this, so here goes: TwoToThreeMap is a file adapted from
			// FieldWorks Ethnologue\Data\iso-639-3_20080804.tab, by discarding all but the first column (3-letter
			// ethnologue codes) and the fourth (two-letter IANA codes), and all the rows where the fourth column is empty.
			// I then swapped the columns. So, in this resource, the string before the tab in each line is a 2-letter
			// Iana code, and the string after it is the one we want to return as the corresponding ISO3Code.
			File.WriteAllText(@"isocodes.txt", iso693);
			var twoToThreeLetter = new Dictionary<string, string>();
			foreach (string line in iso693.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split('\t');
				if (items.Length < 4)
					continue;	// protect against garbage downloaded from the web
				if (items[3].Trim().Length == 2) {
					twoToThreeLetter.Add (items[3].Trim(), items[0].Trim());
				}
			}
			var retval = new StringBuilder();
			foreach (KeyValuePair<string,string> item in twoToThreeLetter)
			{
				retval.AppendLine(String.Format("{0}\t{1}", item.Key, item.Value));
			}
			return retval.ToString().Replace("\r\n", "\n");
		}

		internal bool AreFilesDifferent(string oldfile, string newfile)
		{
			// return true if files are different, false if the same
			var sha = new SHA256Managed();
			byte[] checksum_oldfile = sha.ComputeHash(Encoding.UTF8.GetBytes(oldfile));
			Console.WriteLine("hash of oldfile is: " + BitConverter.ToString(checksum_oldfile));
			byte[] checksum_newfile = sha.ComputeHash(Encoding.UTF8.GetBytes(newfile));
			Console.WriteLine("hash of newfile is: " + BitConverter.ToString(checksum_newfile));

			return BitConverter.ToString(checksum_oldfile) != BitConverter.ToString(checksum_newfile);
		}
	}
}

