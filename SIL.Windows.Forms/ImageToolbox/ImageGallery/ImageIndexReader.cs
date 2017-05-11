using System;
using System.Collections.Generic;
using System.IO;
using SIL.Code;

namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	/// <summary>
	/// This class handles using the first (header) row of the tab-delimited index to retrieve requested data from the other rows.
	/// For example, it remembers in which column  of the header "filename" appeared so that, given a row describing an image,
	/// it can tell us the filename.
	/// Currently this knows just 3 things: filename, subfolder (optional), and column per search language. It also knows that old
	/// Art Of Reading indexes have a "country" column that should be used in place of "subfolder".
	/// </summary>
	internal class ImageIndexReader
	{
		/// <summary>
		/// The languages for which this index has search tearms
		/// </summary>
		public readonly List<string> LanguageIds = new List<string>();

		private readonly Dictionary<string, int> _columnNameToIndex = new Dictionary<string, int>();
		private static string kfilename = "filename";
		private const string ksubfolder = "subfolder";

		protected ImageIndexReader()
		{

		}
		public ImageIndexReader(StreamReader stream)
		{
			// The file should start with a line that looks like the following:
			//someheader <tab> someotherheader <tab> ... someotherheader <tab> en <tab> id <tab> someotherlanguage <tab>
			var header = stream.ReadLine();
			Require.That(!String.IsNullOrEmpty(header), "The first line of the index must not be empty");

			var headings = header.Split(new[] { '\t' });
			for (var i = 0; i < headings.Length; i++)
			{
				var heading = headings[i].ToLowerInvariant();
				_columnNameToIndex.Add(heading, i);
				//ArtOfReading 3.2 did not have a subfolder field, just "country"
				if (heading == "country" && !_columnNameToIndex.ContainsKey(ksubfolder))
				{
					_columnNameToIndex.Add(ksubfolder, i);
				}
				if (headings[i].Length < 4) // non-language column headers have to be more than three letters
				{
					LanguageIds.Add(heading);
				}
			}

			Require.That(_columnNameToIndex.ContainsKey(kfilename), "The index must have a filename column.");
			Require.That(LanguageIds.Count > 0, "No language ids found in header of index.");
		}

		public virtual string GetSubFolderOrEmpty(string[] fields)
		{
			return GetFieldOrEmpty(ksubfolder, fields);
		}

		public virtual string GetFilename(string[] fields)
		{
			return fields[_columnNameToIndex[kfilename]].Trim();
		}

		public virtual string GetImageRelativePath(string[] fields)
		{
			return Path.Combine(GetSubFolderOrEmpty(fields), GetFilename(fields));
		}


		public virtual string GetCSVOfKeywordsOrEmpty(string languageId, string[] fields)
		{
			return GetFieldOrEmpty(languageId, fields);
		}

		private string GetFieldOrEmpty(string key, string[] fields)
		{
			if (_columnNameToIndex.ContainsKey(key.ToLowerInvariant()))
			{
				var i = _columnNameToIndex[key.ToLowerInvariant()];
				if (i < fields.Length) // a given row might not necessarily have as many columns as the header row.
				{
					return fields[i];
				}
			}
			return "";
		}

		public static ImageIndexReader FromFile(string indexPath)
		{
			string headerLine = "";

			using (var reader = File.OpenText(indexPath))
			{
				headerLine = reader.ReadLine();
				reader.Close();
			}
			using (var reader = File.OpenText(indexPath))
			{
				if (headerLine.ToLowerInvariant().Contains(kfilename))
					return new ImageIndexReader(reader);
				else
					return new ArtOfReadingOriginalBilingualIndexReader();
			}
		}
	}

	/// <summary>
	/// Provide index reader for the original AOR index that had only two fields: the filename that used mac-style paths,
	/// and a csv that combined English and Indonesian terms:
	/// e.g
	/// Brazil:B-2-23.tif	"arrow, bow, weapon, anak panah, panah, senjata"
	/// Brazil:B-3-1.tif	"grass, man, people, rumput, manusia, orang, orang-orang"
	/// </summary>
	internal class ArtOfReadingOriginalBilingualIndexReader : ImageIndexReader
	{
		public ArtOfReadingOriginalBilingualIndexReader()
		{
		}

		public override string GetSubFolderOrEmpty(string[] fields)
		{
			return "";
		}

		public override string GetImageRelativePath(string[] fields)
		{
			Require.That(fields.Length == 2);
			return fields[0].Replace(':', Path.DirectorySeparatorChar);
		}

		public override string GetCSVOfKeywordsOrEmpty(string languageId, string[] fields)
		{
			Require.That(fields.Length == 2);
			return fields[1].Trim('"');
		}
	}
}
