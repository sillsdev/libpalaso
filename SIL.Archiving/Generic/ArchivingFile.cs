using System;
using System.IO;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary>A file to add to the archive</summary>
	public class ArchivingFile
	{
		private readonly string _fullName;
		private string _fileName;
		private string _fileSize; // in KB
		private string _mimeType;
		private LanguageStringCollection _descriptions;
		private ArchiveAccessProtocol _accessProtocol;

		/// <summary>If this file contains information about another file, put the name of the other file here</summary>
		public string DescribesAnotherFile;

		/// <summary>Constructor</summary>
		/// <param name="fullFileNameAndPath"></param>
		public ArchivingFile(string fullFileNameAndPath)
		{
			// check if the path is correct
			if (!File.Exists(fullFileNameAndPath))
				throw new FileNotFoundException(fullFileNameAndPath);

			_fullName = fullFileNameAndPath;
			_descriptions = new LanguageStringCollection();
		}

		/// <summary>Constructor</summary>
		public ArchivingFile(ArchivingFile file) : this(file.FullName)
		{
			DescribesAnotherFile = file.DescribesAnotherFile;
			AccessProtocol = file.AccessProtocol;
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

		/// <summary />
		public ArchiveAccessProtocol AccessProtocol
		{
			get { return _accessProtocol; }
			set { _accessProtocol = value; }
		}

		/// <summary />
		public string AccessCode { get; set; }

		/// <summary>Return type strings consistent with the requirements of the archiving format</summary>
		public virtual string GetTypeDescription()
		{
			throw new NotImplementedException("GetTypeDescription");
		}

		/// <summary>Full path and file name</summary>
		public string FullName
		{
			get { return _fullName; }
		}

		/// <summary>Just the file name</summary>
		public string FileName
		{
			get
			{
				if (string.IsNullOrEmpty(_fileName))
					_fileName = (new FileInfo(_fullName)).Name;

				return _fileName;
			}
			set
			{
				// make sure the extension is the same
				var extension = (new FileInfo(_fullName)).Extension;
				_fileName = value;
				if (!_fileName.EndsWith(extension))
					_fileName += extension;
			}
		}
	}
}
