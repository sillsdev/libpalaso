using System;

namespace SIL.Lift
{
	///<summary></summary>
	public class LiftFormatException : ApplicationException
	{
		private string _filePath;
		///<summary>
		/// Constructor.
		///</summary>
		public LiftFormatException(string message) :base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LiftFormatException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		///<summary></summary>
		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}
	}
	///<summary></summary>
	public class BadUpdateFileException :ApplicationException
	{
		private readonly string _pathToOldFile;
		private readonly string _pathToNewFile;

		///<summary>
		/// Constructor.
		///</summary>
		public BadUpdateFileException(string pathToOldFile, string pathToNewFile, Exception innerException)
			: base("Error merging lift", innerException)
		{
			_pathToOldFile = pathToOldFile;
			_pathToNewFile = pathToNewFile;
		}

		///<summary></summary>
		public string PathToOldFile
		{
			get { return _pathToOldFile; }
		}

		///<summary></summary>
		public string PathToNewFile
		{
			get { return _pathToNewFile; }
		}
	}
}
