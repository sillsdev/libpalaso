using System;
using System.Collections.Generic;
using System.IO;
using SIL.Code;

namespace SIL.Windows.Forms.ImageGallery
{
	/// <summary>
	/// This class handles using the first (header) row of the tab-delimeted index to retrieve requested data from the other rows.
	/// For example, it remembers in which column  of the header "filename" appeared so that, given a row describing an image, 
	/// it can tell us the filename.
	/// Currently this knows just 3 things: filename, subfolder (optional), and column per search language. It also knows that old
	/// Art Of Reading indexes have a "country" column that should be used in place of "subfolder".
	/// </summary>
    internal class ImageIndexLayout
    {
		/// <summary>
		/// The languages for which this index has search tearms
		/// </summary>
        public readonly List<string> LanguageIds = new List<string>();

        private readonly Dictionary<string, int> _columnNameToIndex = new Dictionary<string, int>();

        public ImageIndexLayout(StreamReader stream)
        {
            // The file should start with a line that looks like the following:
            //someheader <tab> someotherheader <tab> ... someotherheader <tab> en <tab> id <tab> someotherlanguage <tab>
            var header = stream.ReadLine();
			Require.That(!String.IsNullOrEmpty(header), "The first line of the index must not be empty");

            var headings = header.Split(new[] {'\t'});
            for(var i = 0; i < headings.Length; i++)
            {
                var heading = headings[i].ToLowerInvariant();
                _columnNameToIndex.Add(heading, i);
                //ArtOfReading 3.2 did not have a subfolder field, just "country"
                if(heading == "country" && !_columnNameToIndex.ContainsKey("subfolder"))
                {
                    _columnNameToIndex.Add("subfolder", i);
                }
                if(headings[i].Length < 4) // non-language column headers have to be more than three letters
                {
                    LanguageIds.Add(heading);
                }
            }

            Require.That(_columnNameToIndex.ContainsKey("filename"), "The index must require a filename column.");
			Require.That(LanguageIds.Count > 0, "No language ids found in header of index.");
		}

        public string GetSubFolderOrEmpty(string[] fields)
        {
            return GetFieldOrEmpty("subfolder", fields);
        }

        public string GetFilename(string[] fields)
        {
            return fields[_columnNameToIndex["filename"]];
        }

        public string GetCSVOfKeywordsOrEmpty(string languageId, string[] fields)
        {
            return GetFieldOrEmpty(languageId, fields);
        }

        private string GetFieldOrEmpty(string key, string[] fields)
        {
            if(_columnNameToIndex.ContainsKey(key.ToLowerInvariant()))
            {
                var i = _columnNameToIndex[key.ToLowerInvariant()];
                if(i < fields.Length) // a given row might not necessarily have as many columns as the header row.
                {
                    return fields[_columnNameToIndex[key.ToLowerInvariant()]];
                }
            }
            return "";
        }
    }
}
