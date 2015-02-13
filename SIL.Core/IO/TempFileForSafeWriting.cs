using System.IO;

namespace SIL.IO
{
	public class TempFileForSafeWriting
	{
		private readonly string _realFilePath;
		private string _tempPath;

		public TempFileForSafeWriting(string realFilePath)
		{
			_realFilePath = realFilePath;
			_tempPath = Path.GetTempFileName();
		}

		public string TempFilePath
		{
			get { return _tempPath; }
		}

		public void WriteWasSuccessful()
		{
			//get it onto the same volume for sure
			string pending = _realFilePath+".pending";
			if(File.Exists(pending))
			{
				File.Delete(pending);
			}
			File.Move(_tempPath, pending);//NB: Replace() is tempting but failes across volumes
			if (File.Exists(_realFilePath))
			{
				File.Replace(pending, _realFilePath, _realFilePath + ".bak");
			}
			else
			{
				File.Move(pending, _realFilePath);
			}
		}
	}
}