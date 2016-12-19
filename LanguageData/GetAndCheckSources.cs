using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace LanguageData
{
	public class GetAndCheckSources
	{
		private string _oldtwotothree;
		private string _oldlanguageindex;
		private string _oldianasubtags;


		private string _newtwotothree;
		private string _newlanguageindex;
		private string _newianasubtags;


		static GetAndCheckSources ()
		{
		}

		// solution to teamcity problems with certificates
		// found in http://stackoverflow.com/questions/4926676/mono-webrequest-fails-with-https
		public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			bool isOk = true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None) {
				for (int i=0; i<chain.ChainStatus.Length; i++) {
					if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build ((X509Certificate2)certificate);
						if (!chainIsValid) {
							isOk = false;
						}
					}
				}
			}
			return isOk;
		}

		public void GetNewSources ()
		{
			// Create web client.
			ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
			WebClient client = new WebClient ();

			// Download strings.
			string newiso693 = client.DownloadString ("http://www-01.sil.org/iso639-3/iso-639-3.tab");
			string lastmod_iso693 = client.ResponseHeaders["Last-Modified"];

			_newlanguageindex = client.DownloadString ("https://www.ethnologue.com/codes/LanguageIndex.tab");
			string lastmod_languageindex = client.ResponseHeaders["Last-Modified"];

			_newianasubtags = client.DownloadString ("http://www.iana.org/assignments/language-subtag-registry/language-subtag-registry");
			string lastmod_ianasubtag = client.ResponseHeaders["Last-Modified"];

			Console.WriteLine ("IANA subtags last modified: " + lastmod_ianasubtag);
			Console.WriteLine ("Ethnologue index last modified: " + lastmod_languageindex);
			Console.WriteLine ("ISO693-3 table last modified: " + lastmod_iso693);

			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"LastModified.txt"))
			{
				file.WriteLine ("IANA subtags last modified: " + lastmod_ianasubtag);
				file.WriteLine ("Ethnologue index last modified: " + lastmod_languageindex);
				file.WriteLine ("ISO693-3 table last modified: " + lastmod_iso693);
			}

			_newtwotothree = GenerateTwoToThreeCodes (newiso693);
		}

		public void GetOldSources (string input_dir)
		{
			_oldtwotothree = File.ReadAllText (Path.Combine (input_dir, @"TwoToThreeCodes.txt"));
			_oldlanguageindex = File.ReadAllText (Path.Combine (input_dir, @"LanguageIndex.txt"));
			_oldianasubtags = File.ReadAllText (Path.Combine (input_dir, @"ianaSubtagRegistry.txt"));
		}
		public bool CheckSourcesAreDifferent ()
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

		public void WriteNewFiles(string output_directory)
		{
			if (!Uri.IsWellFormedUriString(output_directory, UriKind.RelativeOrAbsolute))
			{
				throw new Exception();
			}
			string filename = output_directory + Path.DirectorySeparatorChar + "LanguageIndex.txt";
			File.WriteAllText (@filename, _newlanguageindex);
			filename = output_directory + Path.DirectorySeparatorChar + "ianaSubtagRegistry.txt";
			File.WriteAllText (@filename, _newianasubtags);
			filename = output_directory + Path.DirectorySeparatorChar + "TwoToThreeCodes.txt";
			File.WriteAllText (@filename, _newtwotothree);
		}

		public Dictionary<string,string> GetFileStrings(bool newfiles)
		{
			var filestrings = new Dictionary<string,string> ();
			if (newfiles) {
				filestrings.Add ("TwoToThreeCodes.txt", _newtwotothree);
				filestrings.Add ("LanguageIndex.txt", _newlanguageindex);
				filestrings.Add ("ianaSubtagRegistry.txt", _newianasubtags);
			} else {
				filestrings.Add ("TwoToThreeCodes.txt", _oldtwotothree);
				filestrings.Add ("LanguageIndex.txt", _oldlanguageindex);
				filestrings.Add ("ianaSubtagRegistry.txt", _oldianasubtags);
			}
			return filestrings;
		}

		private string GenerateTwoToThreeCodes (string iso693)
		{
			// JohnT: can't find anywhere else to document this, so here goes: TwoToThreeMap is a file adapted from
			// FieldWorks Ethnologue\Data\iso-639-3_20080804.tab, by discarding all but the first column (3-letter
			// ethnologue codes) and the fourth (two-letter IANA codes), and all the rows where the fourth column is empty.
			// I then swapped the columns. So, in this resource, the string before the tab in each line is a 2-letter
			// Iana code, and the string after it is the one we want to return as the corresponding ISO3Code.
			File.WriteAllText (@"isocodes.txt", iso693);
			var twoToThreeLetter = new Dictionary<string, string>();
			foreach (string line in iso693.Replace("\r\n", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] items = line.Split('\t');
				if (items [3].Trim ().Length == 2) {
					twoToThreeLetter.Add (items [3].Trim (), items [0].Trim ());
				}
			}
			string retval = "";
			foreach (KeyValuePair<string,string> item in twoToThreeLetter)
			{
				retval += String.Format("{0}\t{1}\n", item.Key, item.Value);
			}
			return retval;
		}

		private bool AreFilesDifferent(string oldfile, string newfile)
		{
			// return true if files are different, false if the same
			var sha = new SHA256Managed();
			byte[] checksum_oldfile = sha.ComputeHash (Encoding.UTF8.GetBytes(oldfile));
			Console.WriteLine ("hash of oldfile is: " + BitConverter.ToString(checksum_oldfile));
			byte[] checksum_newfile = sha.ComputeHash (Encoding.UTF8.GetBytes(newfile));
			Console.WriteLine ("hash of newfile is: " + BitConverter.ToString(checksum_newfile));

			return BitConverter.ToString(checksum_oldfile) != BitConverter.ToString(checksum_newfile);
		}
	}
}

