
using System.IO;
using SIL.Archiving.Generic;

namespace SIL.Archiving.IMDI
{
	/// <summary>A file to add to the archive</summary>
	public class IMDIFile : ArchivingFile
	{
		private string _generalType;
		private string _normalizedName;

		/// <summary>Constructor</summary>
		/// <param name="fullFileNameAndPath"></param>
		public IMDIFile(string fullFileNameAndPath) : base(fullFileNameAndPath)
		{
		}

		protected override string GetTypeDescription()
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

		/// <summary>File name normalized for archive package</summary>
		public string NormalizedName
		{
			get
			{
				if (string.IsNullOrEmpty(_normalizedName))
				{
					_normalizedName = IMDIArchivingDlgViewModel.NormalizeFileName(FileName);
				}
				return _normalizedName;
			}
		}

		/// <summary>Copies the file to the archive package</summary>
		public void Copy(string contributorDirectoryName)
		{
			var newFileName = Path.Combine(contributorDirectoryName, NormalizedName);
			File.Copy(FullName, newFileName);
		}

		/// <summary>Is this considered a media file</summary>
		public bool IsMediaFile
		{
			get
			{
				switch (GetTypeDescription())
				{
					case "image":
					case "audio":
					case "video":
						return true;

					default:
						return false;
				}
			}
		}

		/// <summary>Is this considered a written resource</summary>
		public bool IsWrittenResource
		{
			get { return !IsMediaFile; }
		}
	}
}
