
using System;
using System.IO;

namespace SIL.Archiving.Generic
{
	/// <summary>A file to add to the archive</summary>
	public class ArchivingFile
	{
		private readonly string _fullName;
		private string _fileSize; // in KB
		private string _mimeType;
		private string _generalType;

		/// <summary>Constructor</summary>
		/// <param name="fullFileNameAndPath"></param>
		public ArchivingFile(string fullFileNameAndPath)
		{
			// check if the path is correct
			if (!File.Exists(fullFileNameAndPath))
				throw new FileNotFoundException(fullFileNameAndPath);

			_fullName = fullFileNameAndPath;

			// get the general type (image, audo, video, document, text, etc.)

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
			get
			{
				if (string.IsNullOrEmpty(_generalType))
				{
					var mime = MimeType;

					if (string.IsNullOrEmpty(mime))
						_generalType = "document";
					else if (mime.StartsWith("image"))
						_generalType = "image";
					else if (mime.StartsWith("audio"))
						_generalType = "audio";
					else if (mime.StartsWith("video"))
						_generalType = "video";
					else if (mime.StartsWith("text"))
						_generalType = "text";
					else
						_generalType = "unspecified";
				}
				return _generalType;
			}
		}
	}
}
