using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using SIL.IO;
using static System.IO.Path;

namespace SIL.TestUtilities
{
	public class TempLiftFile : TempFile
	{
		private const string kLiftFileExt = ".lift";
		
		public TempLiftFile(string xmlOfEntries)
			: this(xmlOfEntries, /*LiftIO.Validation.Validator.LiftVersion*/ "0.12")
		{
		}
		public TempLiftFile(string xmlOfEntries, string claimedLiftVersion)
			: this(null, xmlOfEntries, claimedLiftVersion)
		{
		}

		public TempLiftFile(TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(dontMakeMeAFileAndDontSetPath: true)
		{
			Path = parentFolder != null
				? parentFolder.GetPathForNewTempFile(false) + kLiftFileExt
				: Combine(GetTempPath(), GetRandomFileName() + kLiftFileExt);

			WriteContents(xmlOfEntries, claimedLiftVersion);
		}

		public TempLiftFile(string fileName, TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(dontMakeMeAFileAndDontSetPath: true)
		{
			Path = parentFolder.Combine(fileName);

			WriteContents(xmlOfEntries, claimedLiftVersion);
		}

		private TempLiftFile()
			: base(dontMakeMeAFileAndDontSetPath: true)
		{
		}

		/// <summary>
		/// Create a TempLiftFile based on a pre-existing file, which will be deleted when this is disposed.
		/// </summary>
		[PublicAPI]
		public new static TempLiftFile TrackExisting(string path)
		{
			Debug.Assert(File.Exists(path));
			TempLiftFile t = new TempLiftFile();
			t.Path = path;
			return t;
		}

		private void WriteContents(string xmlOfEntries, string claimedLiftVersion)
		{
			var liftContents = "<?xml version='1.0' encoding='utf-8'?>" +
				$"<lift version='{claimedLiftVersion}'>{xmlOfEntries}</lift>";
			RobustFile.WriteAllText(Path, liftContents);
		}
	}
}