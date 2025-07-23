using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using SIL.IO;

namespace SIL.TestUtilities
{
	public class TempLiftFile : TempFile
	{
		private const string LiftFileExt = ".lift";
		
		public TempLiftFile(string xmlOfEntries)
			: this(xmlOfEntries, /*LiftIO.Validation.Validator.LiftVersion*/ "0.12")
		{
		}
		public TempLiftFile(string xmlOfEntries, string claimedLiftVersion)
			: this(null, xmlOfEntries, claimedLiftVersion)
		{
		}

		public TempLiftFile(TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			if (parentFolder != null)
			{
				Path = parentFolder.GetPathForNewTempFile(false) + LiftFileExt;
			}
			else
			{
				Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName() + LiftFileExt);
			}

			string liftContents =
				$"<?xml version='1.0' encoding='utf-8'?><lift version='{claimedLiftVersion}'>{xmlOfEntries}</lift>";
			RobustFile.WriteAllText(Path, liftContents);
		}

		public TempLiftFile(string fileName, TemporaryFolder parentFolder, string xmlOfEntries, string claimedLiftVersion)
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
		{
			Path = parentFolder.Combine(fileName);

			string liftContents =
				$"<?xml version='1.0' encoding='utf-8'?><lift version='{claimedLiftVersion}'>{xmlOfEntries}</lift>";
			RobustFile.WriteAllText(Path, liftContents);
		}

		private TempLiftFile()
			: base(true) // True means "I'll set the pathname, thank you very much." Otherwise, the temp one 'false' creates will stay forever, and fill the hard drive up.
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
	}
}