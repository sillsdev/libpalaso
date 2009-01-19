using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Palaso.Extensions;

namespace Palaso.UI.WindowsForms.ImageGallery
{
	public interface IImageCollection
	{
		IList<object> GetMatchingPictures(string keywords);

		/// <summary>
		/// The imageTOken here could be a path or whatever, the client doesn't need to know or care
		/// </summary>
		/// <param name="imageToken"></param>
		/// <returns></returns>
		Image GetThumbNail(object imageToken);

		IList<string> GetPathsFromResults(IList<object> results, bool limitToThoseActuallyAvailable);
	}

	public class ArtOfReadingImageCollection :IImageCollection
	{
		private Dictionary<string, List<string>> _index;

		public ArtOfReadingImageCollection()
		{
		   _index = new Dictionary<string, List<string>>();
		}

		public string RootImagePath { get; set; }

		public void LoadIndex(string indexFilePath)
		{

			/*            string path = _fileLocator.LocateFile(_config.IndexFileName, "picture index");
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				throw new ConfigurationException("Could not load picture index file.");
			}
*/
			using (var f = File.OpenText(indexFilePath))
			{
				while (!f.EndOfStream)
				{
					var line = f.ReadLine();
					var parts = line.Split(new char[] { '\t' });
					Debug.Assert(parts.Length == 2);
					if (parts.Length != 2)
						continue;
					var fileName = parts[0];
					var keyString = parts[1].Trim(new char[] { ' ', '"' });//some have quotes, some don't
					var keys = keyString.SplitTrimmed(',');
					foreach (var key in keys)
					{
						_index.GetOrCreate(key).Add(fileName);
					}
				}
			}
		}

		public IList<object> GetMatchingPictures(string keywords)
		{
			return GetMatchingPictures(keywords.SplitTrimmed(' '));
		}

		public Image GetThumbNail(object imageToken)
		{
			throw new System.NotImplementedException();
		}


		public IList<string> GetPathsFromResults(IList<object> results, bool limitToThoseActuallyAvailable)
		{
			List<string> paths= new List<string>();
			foreach (var macPath in results)
			{
				var path = Path.Combine(RootImagePath, ((string) macPath).Replace(':', Path.DirectorySeparatorChar));
				if (!limitToThoseActuallyAvailable || File.Exists(path))
				{
					paths.Add(path);
				}
			}
			return paths;
		}

		private IList<object> GetMatchingPictures(IEnumerable<string> keywords)
		{
			List<string> pictures = new List<string>();
			foreach (var key in keywords)
			{
				List<string> picturesForThisKey = new List<string>();

				if (_index.TryGetValue(key, out picturesForThisKey))
				{
					pictures.AddRange(picturesForThisKey);
				}
			}
			var results = new List<object>();
			pictures.Distinct().ForEach(p => results.Add(p));
			return results;
		}


	}
}
