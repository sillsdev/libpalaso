
using System;
using System.IO;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary>A file to add to the archive</summary>
	public interface IArchivingFile
	{
		/// <summary />
		string FileSize { get; }
		/// <summary />
		string MimeType { get; }
		/// <summary />
		string GeneralType { get; }
		/// <summary />
		LanguageStringCollection Descriptions { get; }
		/// <summary />
		IAccessProtocol AccessProtocol { get; set; }
	}

	/// <summary>A file to add to the archive</summary>
	public abstract class ArchivingFile : IArchivingFile
	{
		protected readonly string _fullName;
		protected string _fileSize; // in KB
		protected string _mimeType;
		protected LanguageStringCollection _descriptions;
		protected IAccessProtocol _accessProtocol;

		/// <summary>If this file contains information about another file, put the name of the other file here</summary>
		public string DescribesAnotherFile;

		/// <summary>Constructor</summary>
		/// <param name="fullFileNameAndPath"></param>
		protected ArchivingFile(string fullFileNameAndPath)
		{
			// check if the path is correct
			if (!File.Exists(fullFileNameAndPath))
				throw new FileNotFoundException(fullFileNameAndPath);

			_fullName = fullFileNameAndPath;
			_descriptions = new LanguageStringCollection();
		}

		/// <summary>Returns the file size in KB as a string</summary>
		public string FileSize
		{
			get
			{
				if (string.IsNullOrEmpty(_fileSize))
				{
					FileInfo fi = new FileInfo(_fullName);

					// get the file size
					var fileSize = ((decimal)fi.Length) / 1024; // in KB

					// do not report zero KB
					if (fileSize < 1)
						fileSize = 1;

					_fileSize = string.Format("{0}KB", Math.Round(fileSize, MidpointRounding.AwayFromZero));
				}
				return _fileSize;
			}
		}

		/// <summary>Returns the mime type of the file</summary>
		public string MimeType
		{
			get
			{
				if (string.IsNullOrEmpty(_mimeType))
				{
					_mimeType = FileMimeType.GetMimeTypeFromFileName(_fullName);
				}
				return _mimeType;
			}
		}

		/// <summary>Returns a general file type based on the mime type</summary>
		public string GeneralType
		{
			get { return GetTypeDescription(); }
		}

		/// <summary />
		public LanguageStringCollection Descriptions
		{
			get { return _descriptions; }
		}

		public IAccessProtocol AccessProtocol
		{
			get { return _accessProtocol; }
			set { _accessProtocol = value; }
		}

		/// <summary>Return type strings consistent with the requirements of the archiving format</summary>
		protected abstract string GetTypeDescription();

		/// <summary>Full path and file name</summary>
		public string FullName
		{
			get { return _fullName; }
		}

		/// <summary>Just the file name</summary>
		public string FileName
		{
			get { return (new FileInfo(_fullName)).Name; }
		}
	}
}
